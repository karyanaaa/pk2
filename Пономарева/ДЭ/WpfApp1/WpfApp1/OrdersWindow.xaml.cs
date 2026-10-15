using System.Windows;
using System.Windows.Input;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1
{
    public partial class OrdersWindow : Window
    {
        private readonly User _user;

        public OrdersWindow(User user)
        {
            InitializeComponent();
            _user = user;
            TxtUserInfo.Text = user.FullName;
            LoadOrders();
        }

        private void LoadOrders()
        {
            GridOrders.ItemsSource = OrderService.LoadOrders();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Оформление заказа доступно из каталога (главное окно).",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!(GridOrders.SelectedItem is OrderRow row))
            {
                MessageBox.Show("Выберите заказ.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить заказ №{row.OrderId}?",
                    "Подтверждение", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            OrderService.DeleteOrder(row.OrderId);
            LoadOrders();
        }

        private void GridOrders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(GridOrders.SelectedItem is OrderRow row)) return;
            var wnd = new OrderDetailWindow(row) { Owner = this };
            wnd.ShowDialog();
        }
    }
}