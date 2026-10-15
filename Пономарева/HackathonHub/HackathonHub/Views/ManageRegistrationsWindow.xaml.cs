using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using HackathonHub.Client.Services;

namespace HackathonHub.Client.Views
{
    public partial class ManageRegistrationsWindow : Window
    {
        private readonly ApiService _api = ApiService.Instance;
        private readonly DialogService _dialog = new DialogService();
        private readonly int _hackathonId;

        public ObservableCollection<RegistrationItem> Items { get; }
            = new ObservableCollection<RegistrationItem>();

        public ManageRegistrationsWindow(int hackathonId)
        {
            InitializeComponent();
            _hackathonId = hackathonId;
            RegistrationsList.ItemsSource = Items;
            Loaded += async (_, __) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            try
            {
                var list = await _api.GetHackathonRegistrationsAsync(_hackathonId);
                Items.Clear();

                if (list != null)
                {
                    foreach (var raw in list)
                    {
                        if (raw is JsonElement je)
                        {
                            var item = new RegistrationItem
                            {
                                Id = je.TryGetProperty("id", out var id) ? id.GetInt32() : 0,
                                FullName = je.TryGetProperty("fullName", out var fn) ? fn.GetString() : "",
                                Email = je.TryGetProperty("email", out var em) ? em.GetString() : "",
                                Status = je.TryGetProperty("status", out var st) ? st.GetString() : "pending",
                            };
                            Items.Add(item);
                        }
                    }
                }

                EmptyText.Visibility = Items.Count == 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            catch (System.Exception ex)
            {
                _dialog.ShowError($"Ошибка загрузки заявок: {ex.Message}");
            }
        }

        private async void Approve_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int id) return;

            try
            {
                var resp = await _api.ApproveRegistrationAsync(id);
                if (resp.Success)
                {
                    _dialog.ShowInfo("Заявка одобрена");
                    await LoadAsync();
                }
                else _dialog.ShowError(resp.Message ?? "Ошибка");
            }
            catch (System.Exception ex) { _dialog.ShowError(ex.Message); }
        }

        private async void Reject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int id) return;

            try
            {
                var resp = await _api.RejectRegistrationAsync(id);
                if (resp.Success)
                {
                    _dialog.ShowInfo("Заявка отклонена");
                    await LoadAsync();
                }
                else _dialog.ShowError(resp.Message ?? "Ошибка");
            }
            catch (System.Exception ex) { _dialog.ShowError(ex.Message); }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
            => await LoadAsync();

        private void Close_Click(object sender, RoutedEventArgs e)
            => Close();
    }

    public class RegistrationItem : ViewModels.BaseViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }

        public string StatusText => Status switch
        {
            "pending" => "на рассмотрении",
            "approved" => "принята",
            "rejected" => "отклонена",
            _ => Status
        };

        public Visibility PendingVisibility =>
            Status == "pending" ? Visibility.Visible : Visibility.Collapsed;
    }
}