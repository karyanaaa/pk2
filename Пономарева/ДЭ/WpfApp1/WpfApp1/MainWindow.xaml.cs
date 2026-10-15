using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        // null = неавторизованный
        private User _currentUser;

        // Конструктор для НЕавторизованного (открывается при запуске)
        public MainWindow()
        {
            InitializeComponent();
            _currentUser = null;
            SetupForRole();
            LoadCategories();
            LoadSort();
            LoadCatalog();
            HookFilters();
        }

        // Конструктор для авторизованного (вызывается после логина)
        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            SetupForRole();
            LoadCategories();
            LoadSort();
            LoadCatalog();
            HookFilters();
        }

        private void HookFilters()
        {
            TxtSearch.TextChanged += (s, e) => LoadCatalog();
            CmbCategory.SelectionChanged += (s, e) => LoadCatalog();
            CmbSort.SelectionChanged += (s, e) => LoadCatalog();
        }

        private void SetupForRole()
        {
            bool isAuthorized = _currentUser != null;

            // Фильтры видны только авторизованным
            FilterPanel.Visibility = isAuthorized ? Visibility.Visible : Visibility.Collapsed;

            // Кнопка "Войти" — только неавторизованным
            BtnLogin.Visibility = isAuthorized ? Visibility.Collapsed : Visibility.Visible;

            // Кнопка "Выход" — только авторизованным
            BtnLogout.Visibility = isAuthorized ? Visibility.Visible : Visibility.Collapsed;

            // ФИО в шапке
            if (isAuthorized)
                TxtUserInfo.Text = $"{_currentUser.FullName} ({_currentUser.Role})";
            else
                TxtUserInfo.Text = "";

            // Кнопка "Заказы" — только менеджеру и админу
            if (isAuthorized && _currentUser.Role != "Авторизованный пользователь")
                BtnOrders.Visibility = Visibility.Visible;
            else
                BtnOrders.Visibility = Visibility.Collapsed;
        }

        private void LoadCategories()
        {
            CmbCategory.ItemsSource = ProductService.LoadCategories();
            if (CmbCategory.Items.Count > 0) CmbCategory.SelectedIndex = 0;
        }

        private void LoadSort()
        {
            CmbSort.Items.Add("По цене ↑");
            CmbSort.Items.Add("По цене ↓");
            CmbSort.SelectedIndex = 0;
        }

        private void LoadCatalog()
        {
            // Для неавторизованного — фильтры не применяются
            string search = (_currentUser != null) ? TxtSearch.Text : "";
            string category = (_currentUser != null)
                ? (CmbCategory.SelectedItem as string ?? "Все категории")
                : "Все категории";

            var list = ProductService.LoadCatalog(search, category);

            // Сортировка по умолчанию — по имени
            if (CmbSort.SelectedIndex == 1)
                list.Sort((a, b) => b.FinalPrice.CompareTo(a.FinalPrice));
            else
                list.Sort((a, b) => a.FinalPrice.CompareTo(b.FinalPrice));

            ProductsList.ItemsSource = list;
        }

        private void ProductCard_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var card = border?.DataContext as ProductCard;
            if (card == null) return;

            // Неавторизованный — только просмотр, без оформления
            if (_currentUser == null)
            {
                MessageBox.Show(
                    "Для оформления заказа нужно войти в систему.",
                    "Требуется вход",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var wnd = new ProductWindow(card, _currentUser) { Owner = this };
            wnd.ShowDialog();
            LoadCatalog();
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new OrdersWindow(_currentUser) { Owner = this };
            wnd.ShowDialog();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            if (login.ShowDialog() == true)
            {
                // После успешного входа открываем новое MainWindow с пользователем
                var main = new MainWindow(login.AuthenticatedUser);
                main.Show();
                Close();
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Выход — возвращаемся к неавторизованному виду
            var main = new MainWindow();
            main.Show();
            Close();
        }
    }
}