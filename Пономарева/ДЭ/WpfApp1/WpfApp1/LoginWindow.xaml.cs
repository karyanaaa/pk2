using System.Windows;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1
{
    public partial class LoginWindow : Window
    {
        public User AuthenticatedUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLogin.Text.Trim();

            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Введите логин.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = AuthService.Authenticate(login);
            if (user == null)
            {
                MessageBox.Show("Пользователь с таким логином не найден.",
                    "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AuthenticatedUser = user;
            DialogResult = true;
            Close();
        }
    }
}