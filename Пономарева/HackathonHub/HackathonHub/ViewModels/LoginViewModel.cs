using HackathonHub.Client.Models;
using HackathonHub.Client.Services;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HackathonHub.Client.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly ApiService _api = ApiService.Instance;
        private string _email;
        private string _password;
        private string _errorMessage;
        private bool _isLoading;

        public string Email { get => _email; set => SetProperty(ref _email, value); }
        public string Password { get => _password; set => SetProperty(ref _password, value); }

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

        public ICommand LoginCommand { get; }
        public ICommand OpenRegisterCommand { get; }
        public ICommand ForgotPasswordCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new AsyncRelayCommand(async _ => await LoginAsync(), _ => !IsLoading);
            OpenRegisterCommand = new RelayCommand(_ => OpenRegister());
            ForgotPasswordCommand = new RelayCommand(_ => OpenForgotPassword());
        }

        private async Task LoginAsync()
        {
            ErrorMessage = null;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Заполните все поля";
                return;
            }

            IsLoading = true;
            try
            {
                var response = await _api.LoginAsync(Email, Password);

                if (!response.Success)
                {
                    ErrorMessage = response.Message ?? "Неверный email или пароль";
                    return;
                }

                string json;
                if (response.Data is System.Text.Json.JsonElement je)
                    json = je.GetRawText();
                else
                    json = Newtonsoft.Json.JsonConvert.SerializeObject(response.Data);

                var data = JObject.Parse(json);
                var token = data["token"]?.ToString();
                var user = data["user"]?.ToObject<User>();

                if (!string.IsNullOrEmpty(token) && user != null)
                {
                    _api.SetToken(token, user);
                    OpenMainWindow();
                }
                else
                {
                    ErrorMessage = "Сервер вернул некорректный ответ";
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

        private void OpenMainWindow()
        {
            try
            {
                var main = new Views.MainWindow();
                main.Show();
                Application.Current.MainWindow = main;

                // Закрываем все остальные окна (Login, Register и т.д.)
                var windows = Application.Current.Windows.Cast<Window>().ToList();
                foreach (var w in windows)
                {
                    if (w != main) w.Close();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка открытия главного окна: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenRegister()
        {
            var reg = new Views.RegisterWindow();
            reg.Show();
            foreach (Window w in Application.Current.Windows)
            {
                if (!(w is Views.RegisterWindow)) w.Close();
            }
        }

        private void OpenForgotPassword()
        {
            var fp = new Views.ForgotPasswordWindow();
            fp.Show();
            foreach (Window w in Application.Current.Windows)
            {
                if (!(w is Views.ForgotPasswordWindow)) w.Close();
            }
        }
    }
}