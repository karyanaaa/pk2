using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HackathonHub.Client.Services;

namespace HackathonHub.Client.ViewModels
{
    public class ForgotPasswordViewModel : BaseViewModel
    {
        private readonly ApiService _api = ApiService.Instance;
        private readonly EmailService _emailService = new EmailService();

        private string _email;
        private string _statusMessage;
        private bool _isSuccess;
        private bool _isLoading;

        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                SetProperty(ref _statusMessage, value);
                OnPropertyChanged(nameof(HasStatus));
                OnPropertyChanged(nameof(HasStatusVisibility));
            }
        }
        public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);
        public Visibility HasStatusVisibility => HasStatus ? Visibility.Visible : Visibility.Collapsed;

        public bool IsSuccess
        {
            get => _isSuccess;
            set
            {
                SetProperty(ref _isSuccess, value);
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        public string StatusColor => IsSuccess ? "#4ade80" : "#ff6b7d";
        public string StatusBackground => IsSuccess ? "#334ade80" : "#33ff6b7d";

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                OnPropertyChanged(nameof(IsLoadingVisibility));
            }
        }
        public Visibility IsLoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;

        public ICommand SendResetCommand { get; }
        public ICommand BackCommand { get; }

        public ForgotPasswordViewModel()
        {
            SendResetCommand = new AsyncRelayCommand(async _ => await SendResetAsync(),
                _ => !IsLoading && !string.IsNullOrWhiteSpace(Email));
            BackCommand = new RelayCommand(_ => Back());
        }

        private async Task SendResetAsync()
        {
            IsLoading = true;
            StatusMessage = null;

            try
            {
                if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
                {
                    IsSuccess = false;
                    StatusMessage = "Введите корректный email";
                    return;
                }

                var response = await _api.ForgotPasswordAsync(Email);

                if (!response.Success)
                {
                    IsSuccess = false;
                    StatusMessage = response.Message ?? "Пользователь с таким email не найден";
                    return;
                }

                // Извлекаем токен
                string token = null;
                if (response.Data != null)
                {
                    string json;
                    if (response.Data is System.Text.Json.JsonElement je)
                        json = je.GetRawText();
                    else
                        json = Newtonsoft.Json.JsonConvert.SerializeObject(response.Data);

                    var obj = Newtonsoft.Json.Linq.JObject.Parse(json);
                    token = obj["resetToken"]?.ToString();
                }

                if (string.IsNullOrEmpty(token))
                {
                    IsSuccess = false;
                    StatusMessage = "Не удалось получить токен сброса";
                    return;
                }

                // Пытаемся отправить письмо, но если не получится — не падаем
                try
                {
                    await _emailService.SendPasswordResetEmailAsync(Email, token);
                    IsSuccess = true;
                    StatusMessage = $"Инструкция по сбросу пароля отправлена на {Email}. Проверьте почту.";
                }
                catch
                {
                    IsSuccess = true;
                    StatusMessage = $"Токен для сброса пароля: {token}";
                }
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                StatusMessage = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Back()
        {
            var login = new Views.LoginWindow();
            login.Show();
            foreach (Window w in Application.Current.Windows)
            {
                if (!(w is Views.LoginWindow)) w.Close();
            }
        }
    }
}