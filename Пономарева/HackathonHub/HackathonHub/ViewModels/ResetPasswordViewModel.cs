using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HackathonHub.Client.Services;

namespace HackathonHub.Client.ViewModels
{
    public class ResetPasswordViewModel : BaseViewModel
    {
        private readonly ApiService _api = ApiService.Instance;
        private readonly DialogService _dialog = new DialogService();

        private string _token;
        private string _newPassword;
        private string _confirmPassword;
        private string _errorMessage;
        private bool _isLoading;

        public string Token { get => _token; set => SetProperty(ref _token, value); }
        public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }
        public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                OnPropertyChanged(nameof(HasError));
                OnPropertyChanged(nameof(HasErrorVisibility));
            }
        }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public Visibility HasErrorVisibility => HasError ? Visibility.Visible : Visibility.Collapsed;

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

        public ICommand ResetCommand { get; }
        public ICommand BackCommand { get; }

        public ResetPasswordViewModel(string token = null)
        {
            _token = token;
            ResetCommand = new AsyncRelayCommand(async _ => await ResetAsync(), _ => !IsLoading);
            BackCommand = new RelayCommand(_ => Back());
        }

        private async Task ResetAsync()
        {
            ErrorMessage = null;

            if (string.IsNullOrWhiteSpace(Token))
            {
                ErrorMessage = "Неверный токен сброса";
                return;
            }
            if (NewPassword?.Length < 8)
            {
                ErrorMessage = "Пароль должен содержать минимум 8 символов";
                return;
            }
            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают";
                return;
            }

            IsLoading = true;
            try
            {
                var response = await _api.ResetPasswordAsync(Token, NewPassword);
                if (response.Success)
                {
                    _dialog.ShowInfo("Пароль успешно изменён! Войдите с новым паролем.");
                    Back();
                }
                else
                {
                    ErrorMessage = response.Message ?? "Ошибка сброса пароля";
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
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