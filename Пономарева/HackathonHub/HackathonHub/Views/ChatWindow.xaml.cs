using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using HackathonHub.Client.Models;
using HackathonHub.Client.Services;

namespace HackathonHub.Client.Views
{
    public partial class ChatWindow : Window
    {
        private readonly ApiService _api = ApiService.Instance;
        private readonly DialogService _dialog = new DialogService();
        private readonly int _hackathonId;
        private readonly DispatcherTimer _timer;

        public ObservableCollection<ChatMessage> Messages { get; }
            = new ObservableCollection<ChatMessage>();

        public ChatWindow(int hackathonId, string hackathonTitle)
        {
            InitializeComponent();
            _hackathonId = hackathonId;
            HeaderText.Text = $"💬 Чат: {hackathonTitle}";
            MessagesList.ItemsSource = Messages;

            Loaded += async (_, __) => await LoadMessagesAsync();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _timer.Tick += async (_, __) => await LoadMessagesAsync();
            _timer.Start();
            Closed += (_, __) => _timer.Stop();
        }

        private async System.Threading.Tasks.Task LoadMessagesAsync()
        {
            try
            {
                var list = await _api.GetChatMessagesAsync(_hackathonId);
                if (list == null) return;
                Messages.Clear();
                foreach (var m in list) Messages.Add(m);
                MessagesScroll?.ScrollToEnd();
            }
            catch (System.Exception ex)
            {
                // Тихо игнорируем — сервер уже вернул fail
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            await SendCurrentMessageAsync();
        }

        private async void MessageBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SendCurrentMessageAsync();
                e.Handled = true;
            }
        }

        private async System.Threading.Tasks.Task SendCurrentMessageAsync()
        {
            var text = MessageBox.Text?.Trim();
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                var resp = await _api.SendChatMessageAsync(_hackathonId, text);
                if (resp.Success)
                {
                    MessageBox.Text = "";
                    await LoadMessagesAsync();
                }
                else _dialog.ShowError(resp.Message ?? "Ошибка отправки");
            }
            catch (Exception ex) { _dialog.ShowError(ex.Message); }
        }
    }
}