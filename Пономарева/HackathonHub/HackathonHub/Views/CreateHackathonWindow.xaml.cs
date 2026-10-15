using System;
using System.Windows;
using HackathonHub.Client.Models;
using HackathonHub.Client.Services;

namespace HackathonHub.Client.Views
{
    public partial class CreateHackathonWindow : Window
    {
        private readonly ApiService _api = ApiService.Instance;
        private readonly DialogService _dialog = new DialogService();

        public CreateHackathonWindow() => InitializeComponent();

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text) ||
                string.IsNullOrWhiteSpace(DescBox.Text) ||
                StartDatePicker.SelectedDate == null ||
                EndDatePicker.SelectedDate == null ||
                RegDeadlinePicker.SelectedDate == null)
            {
                _dialog.ShowWarning("Заполните все обязательные поля");
                return;
            }

            if (StartDatePicker.SelectedDate >= EndDatePicker.SelectedDate)
            {
                _dialog.ShowWarning("Дата старта должна быть раньше даты окончания");
                return;
            }

            var hackathon = new Hackathon
            {
                Title = TitleBox.Text,
                Description = DescBox.Text,
                StartDate = StartDatePicker.SelectedDate.Value,
                EndDate = EndDatePicker.SelectedDate.Value,
                RegistrationDeadline = RegDeadlinePicker.SelectedDate.Value,
                Status = "open",
                MaxTeams = int.TryParse(MaxTeamsBox.Text, out var m) ? m : 50,
                OrganizerId = _api.CurrentUser.Id
            };

            try
            {
                var resp = await _api.CreateHackathonAsync(hackathon);
                if (resp.Success)
                {
                    _dialog.ShowInfo("Хакатон успешно создан!");
                    DialogResult = true;
                    Close();
                }
                else
                {
                    _dialog.ShowError(resp.Message ?? "Ошибка создания");
                }
            }
            catch (Exception ex)
            {
                _dialog.ShowError(ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}