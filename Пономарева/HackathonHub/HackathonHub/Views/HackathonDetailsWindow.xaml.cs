using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using HackathonHub.Client.Models;
using HackathonHub.Client.Services;

namespace HackathonHub.Client.Views
{
    public partial class HackathonDetailsWindow : Window
    {
        private readonly Hackathon _hackathon;
        private readonly ApiService _api = ApiService.Instance;
        private readonly DialogService _dialog = new DialogService();
        private ObservableCollection<Team> _teams = new ObservableCollection<Team>();

        public HackathonDetailsWindow(Hackathon h)
        {
            InitializeComponent();
            _hackathon = h;

            TitleText.Text = h.Title ?? "";
            StatusText.Text = $"Статус: {h.Status}";
            DescText.Text = h.Description ?? "";
            StartText.Text = h.StartDate.ToString("dd.MM.yyyy");
            EndText.Text = h.EndDate.ToString("dd.MM.yyyy");
            DeadlineText.Text = h.RegistrationDeadline.ToString("dd.MM.yyyy");

            TaskText.Text = string.IsNullOrWhiteSpace(h.TaskDescription)
                ? "Организатор пока не опубликовал задание."
                : h.TaskDescription;

            Loaded += async (_, __) =>
            {
                await LoadTeamsAsync();
                await LoadMyRegistrationAsync();
            };
            UpdateButtonsVisibility();
        }

        private void UpdateButtonsVisibility()
        {
            var role = _api.CurrentUser?.Role;
            var isParticipant = role == "participant";

            if (CreateTeamBtn != null)
                CreateTeamBtn.Visibility = isParticipant ? Visibility.Visible : Visibility.Collapsed;
            if (JoinTeamBtn != null)
                JoinTeamBtn.Visibility = isParticipant ? Visibility.Visible : Visibility.Collapsed;
            if (UploadProjectBtn != null)
                UploadProjectBtn.Visibility = isParticipant ? Visibility.Visible : Visibility.Collapsed;

            var isOrganizer = role == "organizer" || role == "admin";
            if (DeleteHackathonBtn != null)
                DeleteHackathonBtn.Visibility = isOrganizer ? Visibility.Visible : Visibility.Collapsed;
            if (AddCriterionBtn != null)
                AddCriterionBtn.Visibility = isOrganizer ? Visibility.Visible : Visibility.Collapsed;
            if (ManageCriteriaBtn != null)
                ManageCriteriaBtn.Visibility = isOrganizer ? Visibility.Visible : Visibility.Collapsed;

            if (ApplyBtn != null)
                ApplyBtn.Visibility = isParticipant ? Visibility.Visible : Visibility.Collapsed;
            if (ManageRegistrationsBtn != null)
                ManageRegistrationsBtn.Visibility = isOrganizer ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ManageRegistrations_Click(object sender, RoutedEventArgs e)
        {
            var role = _api.CurrentUser?.Role;
            if (role != "organizer" && role != "admin")
            {
                _dialog.ShowWarning("Только организатор может просматривать заявки");
                return;
            }

            var win = new ManageRegistrationsWindow(_hackathon.Id) { Owner = this };
            win.ShowDialog();
        }

        private async System.Threading.Tasks.Task LoadTeamsAsync()
        {
            try
            {
                var list = await _api.GetTeamsByHackathonAsync(_hackathon.Id);
                _teams = new ObservableCollection<Team>(
                    list ?? new System.Collections.Generic.List<Team>());
                TeamsGrid.ItemsSource = _teams;
            }
            catch (Exception ex)
            {
                _dialog.ShowError($"Не удалось загрузить команды: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadMyRegistrationAsync()
        {
            if (_api.CurrentUser?.Role != "participant") return;
            if (ApplyBtn == null) return;

            try
            {
                var obj = await _api.GetMyRegistrationAsync(_hackathon.Id);
                string status = null;

                if (obj is System.Text.Json.JsonElement je)
                {
                    if (je.ValueKind == System.Text.Json.JsonValueKind.Object &&
                        je.TryGetProperty("status", out var st))
                    {
                        status = st.ValueKind == System.Text.Json.JsonValueKind.Null
                            ? null
                            : st.GetString();
                    }
                }

                switch (status)
                {
                    case "pending":
                        ApplyBtn.Content = "⏳ ЗАЯВКА НА РАССМОТРЕНИИ";
                        ApplyBtn.IsEnabled = false;
                        break;
                    case "approved":
                        ApplyBtn.Content = "✅ ВЫ ЗАРЕГИСТРИРОВАНЫ";
                        ApplyBtn.IsEnabled = false;
                        break;
                    case "rejected":
                        ApplyBtn.Content = "❌ ЗАЯВКА ОТКЛОНЕНА";
                        ApplyBtn.IsEnabled = false;
                        break;
                    default:
                        ApplyBtn.Content = "ПОДАТЬ ЗАЯВКУ";
                        ApplyBtn.IsEnabled = true;
                        break;
                }
            }
            catch
            {
                // тихо игнорируем
            }
        }

        private async void CreateTeam_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateTeamWindow(_hackathon.Id) { Owner = this };
            if (dialog.ShowDialog() == true)
                await LoadTeamsAsync();
        }

        private async void JoinTeam_Click(object sender, RoutedEventArgs e)
        {
            if (TeamsGrid.SelectedItem is not Team team)
            {
                _dialog.ShowWarning("Выберите команду в списке");
                return;
            }

            if (!_dialog.Confirm($"Отправить заявку на вступление в команду \"{team.Name}\"?"))
                return;

            try
            {
                var resp = await _api.RequestJoinTeamAsync(team.Id);
                if (resp.Success)
                    _dialog.ShowInfo("Заявка отправлена капитану!");
                else
                    _dialog.ShowError(resp.Message ?? "Ошибка");
            }
            catch (Exception ex) { _dialog.ShowError(ex.Message); }
        }

        private async void UploadProject_Click(object sender, RoutedEventArgs e)
        {
            var now = DateTime.Now;

            if (now < _hackathon.StartDate)
            {
                _dialog.ShowWarning(
                    $"Загрузка проекта откроется {_hackathon.StartDate:dd.MM.yyyy HH:mm} — " +
                    "дождитесь старта хакатона");
                return;
            }

            var deadline = _hackathon.SubmissionDeadline ?? _hackathon.EndDate;
            if (now > deadline)
            {
                _dialog.ShowWarning("Приём работ завершён");
                return;
            }

            var captainTeam = _teams.FirstOrDefault(t => t.CaptainId == _api.CurrentUser?.Id);
            if (captainTeam == null)
            {
                _dialog.ShowWarning("Вы не являетесь капитаном ни одной команды в этом хакатоне");
                return;
            }

            var dialog = new UploadProjectWindow(captainTeam.Id) { Owner = this };
            if (dialog.ShowDialog() == true)
                await LoadTeamsAsync();
        }

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            var role = _api.CurrentUser?.Role;
            if (role != "participant")
            {
                _dialog.ShowWarning("Только участники могут подавать заявку");
                return;
            }

            var now = DateTime.Now;
            if (now > _hackathon.EndDate)
            {
                _dialog.ShowInfo("Хакатон уже завершён");
                return;
            }
            if (now > _hackathon.RegistrationDeadline)
            {
                _dialog.ShowInfo("Регистрация на этот хакатон закрыта");
                return;
            }

            try
            {
                var resp = await _api.RegisterForHackathonAsync(_hackathon.Id);
                if (resp.Success)
                {
                    _dialog.ShowInfo("Заявка отправлена организатору!");
                    await LoadMyRegistrationAsync();
                }
                else
                {
                    _dialog.ShowError(resp.Message ?? "Ошибка");
                }
            }
            catch (Exception ex)
            {
                _dialog.ShowError(ex.Message);
            }
        }

        private void DeleteHackathon_Click(object sender, RoutedEventArgs e)
        {
            var role = _api.CurrentUser?.Role;
            if (role != "organizer" && role != "admin")
            {
                _dialog.ShowWarning("Только организатор может удалить хакатон");
                return;
            }
            if (_hackathon.OrganizerId != _api.CurrentUser.Id && role != "admin")
            {
                _dialog.ShowWarning("Вы не являетесь организатором этого хакатона");
                return;
            }
            if (!_dialog.Confirm($"Удалить хакатон \"{_hackathon.Title}\"?\nВсе команды и проекты будут удалены."))
                return;

            _ = DeleteHackathonInternalAsync();
        }

        private async System.Threading.Tasks.Task DeleteHackathonInternalAsync()
        {
            try
            {
                var resp = await _api.DeleteHackathonAsync(_hackathon.Id);
                if (resp.Success)
                {
                    _dialog.ShowInfo("Хакатон удалён");
                    DialogResult = true;
                    Close();
                }
                else _dialog.ShowError(resp.Message ?? "Ошибка");
            }
            catch (Exception ex) { _dialog.ShowError(ex.Message); }
        }

        private void AddCriterion_Click(object sender, RoutedEventArgs e)
        {
            var role = _api.CurrentUser?.Role;
            if (role != "organizer" && role != "admin")
            {
                _dialog.ShowWarning("Только организатор может добавлять критерии");
                return;
            }

            var win = new CreateCriterionWindow(_hackathon.Id) { Owner = this };
            if (win.ShowDialog() == true)
            {
                _dialog.ShowInfo("Критерий добавлен. Жюри сможет по нему оценивать.");
            }
        }

        private void ManageCriteria_Click(object sender, RoutedEventArgs e)
        {
            var role = _api.CurrentUser?.Role;
            if (role != "organizer" && role != "admin")
            {
                _dialog.ShowWarning("Только организатор может управлять критериями");
                return;
            }

            var win = new ManageCriteriaWindow(_hackathon.Id) { Owner = this };
            win.ShowDialog();
        }
    }
}