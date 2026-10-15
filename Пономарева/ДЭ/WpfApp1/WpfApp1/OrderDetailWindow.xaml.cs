using System.Linq;
using System.Windows;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1
{
    public partial class OrderDetailWindow : Window
    {
        private readonly OrderRow _order;

        public OrderDetailWindow(OrderRow order)
        {
            InitializeComponent();
            _order = order;

            TxtHeader.Text = $"Заказ №{order.OrderId} от {order.OrderDate:dd.MM.yyyy}";
            TxtCustomer.Text = $"Клиент: {order.CustomerName}";

            var items = OrderService.LoadOrderItems(order.OrderId);
            GridItems.ItemsSource = items;

            decimal total = items.Sum(i => i.Sum);
            TxtTotal.Text = $"ИТОГО: {total:N2} ₽";
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => Close();
    }
}