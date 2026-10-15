using System;
using System.Windows;
using HackathonHub.Client.Models;
using HackathonHub.Client.Services;

namespace HackathonHub.Client.Views
{
    public partial class CreateTeamWindow : Window
    {
        private readonly ApiService _api = ApiService.Instance;
        private readonly DialogService _dialog = new DialogService();
        private readonly int _hackathonId;

        public CreateTeamWindow(int hackathonId)
        {
            InitializeComponent();
            _hackathonId = hackathonId;
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                _dialog.ShowWarning("Введите название команды");
                return;
            }

            var team = new Team
            {
                HackathonId = _hackathonId,
                Name = NameBox.Text.Trim(),
                Description = DescBox.Text?.Trim(),
            };

            try
            {
                var resp = await _api.CreateTeamAsync(team);
                if (resp.Success)
                {
                    _dialog.ShowInfo("Команда создана!");
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