using System;
using System.Windows;
using HackathonHub.Client.Models;
using HackathonHub.Client.Services;

namespace HackathonHub.Client.Views
{
    public partial class CreateCriterionWindow : Window
    {
        private readonly ApiService _api = ApiService.Instance;
        private readonly DialogService _dialog = new DialogService();
        private readonly int _hackathonId;
        private bool _isCreating = false;

        public CreateCriterionWindow(int hackathonId)
        {
            InitializeComponent();
            _hackathonId = hackathonId;
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            if (_isCreating) return;
            _isCreating = true;

            try
            {
                if (string.IsNullOrWhiteSpace(NameBox.Text))
                {
                    _dialog.ShowWarning("Введите название критерия");
                    return;
                }

                int maxScore = int.TryParse(MaxScoreBox.Text, out var ms) ? ms : 10;
                double weight = double.TryParse(WeightBox.Text,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var w) ? w : 1.0;

                var criterion = new Criterion
                {
                    HackathonId = _hackathonId,
                    Name = NameBox.Text.Trim(),
                    Description = DescBox.Text?.Trim(),
                    MaxScore = maxScore,
                    Weight = (decimal)weight
                };

                var resp = await _api.CreateCriterionAsync(criterion);
                if (resp.Success)
                {
                    _dialog.ShowInfo("Критерий добавлен!");
                    SetDialogResultSafe(true);
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
            finally
            {
                _isCreating = false;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SetDialogResultSafe(false);
            Close();
        }

        private void SetDialogResultSafe(bool value)
        {
            if (System.Windows.Interop.ComponentDispatcher.IsThreadModal)
                DialogResult = value;
        }
    }
}