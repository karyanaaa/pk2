using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HackathonHub.Client.Services;

namespace HackathonHub.Client.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly ApiService _api = ApiService.Instance;
        private string _fullName;
        private string _email;
        private string _password;
        private string _confirmPassword;
        private string _errorMessage;
        private bool _isLoading;

        public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }
        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string Password { get => _password; set => SetProperty(ref _password, value); }
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

        public ICommand RegisterCommand { get; }
        public ICommand BackToLoginCommand { get; }

        public RegisterViewModel()
        {
            RegisterCommand = new AsyncRelayCommand(async _ => await RegisterAsync(), _ => !IsLoading);
            BackToLoginCommand = new RelayCommand(_ => BackToLogin());
        }

        private async Task RegisterAsync()
        {
            ErrorMessage = null;
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Заполните все поля";
                return;
            }
            if (Password.Length < 8)
            {
                ErrorMessage = "Пароль должен быть минимум 8 символов";
                return;
            }
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают";
                return;
            }

            IsLoading = true;
            try
            {
                var response = await _api.RegisterAsync(FullName, Email, Password, "participant");
                if (!response.Success)
                {
                    ErrorMessage = response.Message ?? "Ошибка регистрации";
                    return;
                }

                var dialog = new DialogService();
                dialog.ShowInfo("Регистрация успешна! Теперь войдите в систему.", "Успех");
                BackToLogin();
            }
            catch (System.Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally { IsLoading = false; }
        }

        private void BackToLogin()
        {
            var login = new Views.LoginWindow();
            login.Show();
            foreach (Window w in Application.Current.Windows)
                if (!(w is Views.LoginWindow)) w.Close();
            Application.Current.MainWindow = login;
        }
    }
}