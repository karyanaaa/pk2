using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using HackathonHub.Client.Models;
using HackathonHub.Client.Services;

namespace HackathonHub.Client.Views
{
    public partial class ScoreProjectWindow : Window
    {
        private readonly ApiService _api = ApiService.Instance;
        private readonly DialogService _dialog = new DialogService();
        private readonly Project _project;

        public string ProjectTitle => _project?.Title ?? "";
        public string TeamName => _project?.TeamName ?? "";
        public string ProjectDescription => _project?.Description ?? "";

        public ObservableCollection<ScoreItem> Criteria { get; } = new ObservableCollection<ScoreItem>();

        public ScoreProjectWindow(Project project)
        {
            InitializeComponent();
            _project = project;
            DataContext = this;
            Loaded += async (_, __) => await LoadCriteriaAsync();
        }

        private async System.Threading.Tasks.Task LoadCriteriaAsync()
        {
            Criteria.Clear();

            if (_project == null)
            {
                _dialog.ShowError("Проект не передан");
                return;
            }

            int hackathonId = _project.HackathonId ?? 0;

            if (hackathonId == 0)
            {
                // Попробуем вытащить hackathonId через команду проекта
                // (на случай, если сервер не отдал его в /projects/for-jury)
                _dialog.ShowWarning("Не удалось определить хакатон проекта.\n" +
                                    "Критерии не могут быть загружены.");
                return;
            }

            try
            {
                var list = await _api.GetCriteriaAsync(hackathonId);
                if (list == null || list.Count == 0)
                {
                    _dialog.ShowWarning("Организатор ещё не добавил критерии для этого хакатона.\n" +
                                        "Обратитесь к организатору.");
                    return;
                }

                foreach (var c in list)
                {
                    Criteria.Add(new ScoreItem
                    {
                        CriterionId = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        MaxScore = c.MaxScore > 0 ? c.MaxScore : 10,
                        Score = c.MaxScore > 0 ? c.MaxScore / 2 : 5,   // старт — половина макс.
                    });
                }
            }
            catch (System.Exception ex)
            {
                _dialog.ShowError($"Ошибка загрузки критериев: {ex.Message}");
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (Criteria.Count == 0)
            {
                _dialog.ShowWarning("Нет критериев для оценки");
                return;
            }

            try
            {
                foreach (var c in Criteria)
                {
                    await _api.SubmitScoreAsync(new Score
                    {
                        ProjectId = _project.Id,
                        CriterionId = c.CriterionId,
                        Value = c.Score,
                        Comment = c.Comment ?? ""
                    });
                }

                _dialog.ShowInfo("Оценки и комментарии сохранены!");
                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                _dialog.ShowError($"Ошибка: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class ScoreItem : ViewModels.BaseViewModel
    {
        private int _score = 5;
        private string _comment;

        public int CriterionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MaxScore { get; set; } = 10;
        public int Score { get => _score; set => SetProperty(ref _score, value); }
        public string Comment { get => _comment; set => SetProperty(ref _comment, value); }
    }
}