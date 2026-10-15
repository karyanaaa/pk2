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
using demo.Models;        
using demo.Repositories;

namespace demo.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private bool _isCaptchaPassed = false;
        private int _currentAttempts = 0;
        private Image _firstSelectedImage = null;
        private readonly List<string> _correctOrder = new List<string> { "Part1.png", "Part2.png", "Part3.png", "Part4.png" };

        public LoginWindow()
        {
            InitializeComponent();
            ResetCaptcha();
        }

        /// <summary>
        /// Перемешивает фрагменты пазла и сбрасывает состояние капчи.
        /// </summary>
        private void ResetCaptcha()
        {
            _isCaptchaPassed = false;
            _firstSelectedImage = null;

            var images = new List<string> { "Part1.png", "Part2.png", "Part3.png", "Part4.png" };
            Random rnd = new Random();
            images = images.OrderBy(x => rnd.Next()).ToList();

            // Загружаем картинки из ресурсов
            ImgPart1.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{images[0]}"));
            ImgPart2.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{images[1]}"));
            ImgPart3.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{images[2]}"));
            ImgPart4.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{images[3]}"));
        }

        /// <summary>
        /// Обработка кликов по фрагментам пазла для обмена местами.
        /// </summary>
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isCaptchaPassed) return;

            Image clickedImage = sender as Image;
            if (clickedImage == null) return;

            if (_firstSelectedImage == null)
            {
                _firstSelectedImage = clickedImage;
                clickedImage.Opacity = 0.5; // Визуальная подсказка
            }
            else
            {
                // Меняем местами Source
                var temp = _firstSelectedImage.Source;
                _firstSelectedImage.Source = clickedImage.Source;
                clickedImage.Source = temp;

                _firstSelectedImage.Opacity = 1.0;
                _firstSelectedImage = null;

                CheckCaptcha();
            }
        }

        /// <summary>
        /// Проверка правильности сборки пазла.
        /// </summary>
        private void CheckCaptcha()
        {
            var currentOrder = new List<string>
            {
                System.IO.Path.GetFileName(((BitmapImage)ImgPart1.Source).UriSource.ToString()),
                System.IO.Path.GetFileName(((BitmapImage)ImgPart2.Source).UriSource.ToString()),
                System.IO.Path.GetFileName(((BitmapImage)ImgPart3.Source).UriSource.ToString()),
                System.IO.Path.GetFileName(((BitmapImage)ImgPart4.Source).UriSource.ToString())
            };

            if (currentOrder.SequenceEqual(_correctOrder))
            {
                _isCaptchaPassed = true;
                MessageBox.Show("Пазл собран верно! Теперь вы можете войти.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Обработка нажатия кнопки "Войти".
        /// </summary>
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = TxtLogin.Text.Trim();
                string password = TxtPassword.Password.Trim();

                // 1. Проверка заполнения полей
                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Поля 'Логин' и 'Пароль' обязательны для заполнения.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 2. Проверка капчи
                if (!_isCaptchaPassed)
                {
                    _currentAttempts++;
                    HandleFailedAttempt(login, "Пазл собран неверно. Попробуйте еще раз.");
                    return;
                }

                // 3. Поиск пользователя
                User user = UserRepository.GetUserByLogin(login);

                // 4. Проверка пароля
                if (user == null || user.Password != password)
                {
                    _currentAttempts++;
                    HandleFailedAttempt(login, "Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные");
                    return;
                }

                // 5. Проверка на блокировку
                if (user.IsBlocked)
                {
                    MessageBox.Show("Вы заблокированы. Обратитесь к администратору", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                // 6. Успешный вход
                MessageBox.Show("Вы успешно авторизовались", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                _currentAttempts = 0;
                user.FailedAttempts = 0;

                // Открытие окна в зависимости от роли
                if (user.Role == "Администратор")
                {
                    new AdminWindow().Show();
                }
                else
                {
                    new UserWindow(user.Login).Show();
                }
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла непредвиденная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработка неудачной попытки входа.
        /// </summary>
        private void HandleFailedAttempt(string login, string errorMessage)
        {
            MessageBox.Show(errorMessage, "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);

            if (_currentAttempts >= 3)
            {
                UserRepository.BlockUser(login);
                MessageBox.Show("Вы заблокированы. Обратитесь к администратору", "Блокировка", MessageBoxButton.OK, MessageBoxImage.Stop);
            }

            ResetCaptcha();
            TxtPassword.Clear();
        }
    }
}
