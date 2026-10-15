using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using HackathonHub.Client.Models;
using HackathonHub.Client.Services;

namespace HackathonHub.Client.Views
{
    public partial class ManageCriteriaWindow : Window
    {
        private readonly ApiService _api = ApiService.Instance;
        private readonly DialogService _dialog = new DialogService();
        private readonly int _hackathonId;

        public ObservableCollection<Criterion> Criteria { get; }
            = new ObservableCollection<Criterion>();

        public ManageCriteriaWindow(int hackathonId)
        {
            InitializeComponent();
            _hackathonId = hackathonId;
            CriteriaList.ItemsSource = Criteria;
            Loaded += async (_, __) => await LoadCriteriaAsync();
        }

        private async System.Threading.Tasks.Task LoadCriteriaAsync()
        {
            try
            {
                var list = await _api.GetCriteriaAsync(_hackathonId);
                Criteria.Clear();
                if (list != null)
                    foreach (var c in list) Criteria.Add(c);
            }
            catch (System.Exception ex)
            {
                _dialog.ShowError($"Ошибка загрузки: {ex.Message}");
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int id) return;

            if (!_dialog.Confirm("Удалить этот критерий?\nВсе оценки по нему также будут удалены."))
                return;

            try
            {
                var resp = await _api.DeleteCriterionAsync(id);
                if (resp.Success)
                {
                    _dialog.ShowInfo("Критерий удалён");
                    await LoadCriteriaAsync();
                }
                else
                {
                    _dialog.ShowError(resp.Message ?? "Ошибка удаления");
                }
            }
            catch (System.Exception ex)
            {
                _dialog.ShowError(ex.Message);
            }
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            var win = new CreateCriterionWindow(_hackathonId) { Owner = this };
            if (win.ShowDialog() == true)
                await LoadCriteriaAsync();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}