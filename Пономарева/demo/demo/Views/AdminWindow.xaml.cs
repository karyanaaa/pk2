using demo.Models;
using demo.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace demo.Views
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
            UsersGrid.ItemsSource = UserRepository.Users;
        }

        /// <summary>
        /// Добавление нового пользователя.
        /// </summary>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Простой ввод через диалоговое окно (можно заменить на отдельное окно)
                string login = Microsoft.VisualBasic.Interaction.InputBox("Введите логин нового пользователя:", "Добавление пользователя", "");
                if (string.IsNullOrWhiteSpace(login)) return;

                string password = Microsoft.VisualBasic.Interaction.InputBox("Введите пароль:", "Добавление пользователя", "");
                if (string.IsNullOrWhiteSpace(password)) return;

                string role = Microsoft.VisualBasic.Interaction.InputBox("Введите роль (Администратор/Пользователь):", "Добавление пользователя", "Пользователь");
                if (string.IsNullOrWhiteSpace(role)) role = "Пользователь";

                User newUser = new User
                {
                    Login = login.Trim(),
                    Password = password.Trim(),
                    Role = role.Trim(),
                    IsBlocked = false,
                    FailedAttempts = 0
                };

                if (UserRepository.AddUser(newUser))
                {
                    MessageBox.Show("Пользователь успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    UsersGrid.Items.Refresh();
                }
                else
                {
                    MessageBox.Show("Пользователь с таким логином уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Разблокировка выбранного пользователя.
        /// </summary>
        private void BtnUnblock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UsersGrid.SelectedItem is User selectedUser)
                {
                    UserRepository.UnblockUser(selectedUser.Login);
                    UsersGrid.Items.Refresh();
                    MessageBox.Show($"Пользователь {selectedUser.Login} разблокирован.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Выберите пользователя из списка.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при разблокировке: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Выход из учетной записи.
        /// </summary>
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}
