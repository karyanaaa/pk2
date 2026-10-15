using System;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1
{
    public partial class ProductWindow : Window
    {
        private readonly ProductCard _card;
        private readonly User _user;

        public ProductWindow(ProductCard card, User user)
        {
            InitializeComponent();
            _card = card;
            _user = user;

            TxtUserInfo.Text = user.FullName;

            TxtHeader.Text = $"{_card.Manufacturer} | {_card.Name}";
            TxtCategory.Text = $"Категория: {_card.Category}";
            TxtSubcategory.Text = $"Подкатегория: {_card.Subcategory}";
            TxtComposition.Text = $"Состав: {_card.Composition}";
            TxtDescription.Text = $"Описание: {_card.Description}";
            TxtPrice.Text = $"{_card.FinalPrice:N2} ₽";

            LoadImage();
            LoadSizes();
        }

        private void LoadImage()
        {
            try
            {
                if (!string.IsNullOrEmpty(_card.ImagePath))
                {
                    ImgProduct.Source = new BitmapImage(
                        new Uri($"pack://application:,,,/images/{_card.ImagePath}"));
                }
                else
                {
                    ImgProduct.Source = new BitmapImage(
                        new Uri("pack://application:,,,/images/picture.png"));
                }
            }
            catch
            {
                try
                {
                    ImgProduct.Source = new BitmapImage(
                        new Uri("pack://application:,,,/images/picture.png"));
                }
                catch { /* ignore */ }
            }
        }

        private void LoadSizes()
        {
            var list = OrderService.LoadSizes(_card.ProductId);
            CmbSize.ItemsSource = list;
            if (list.Count > 0) CmbSize.SelectedIndex = 0;

            SizesList.ItemsSource = list;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnAddToOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!(CmbSize.SelectedItem is SizeOption opt))
            {
                MessageBox.Show("Выберите размер.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtQuantity.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Некорректное количество.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (qty > opt.Quantity)
            {
                MessageBox.Show($"В наличии только {opt.Quantity} пар.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            OrderService.CreateOrder(_user.FullName, _user.UserId,
                opt.StockItemId, qty, _card.FinalPrice);

            MessageBox.Show("Товар добавлен в заказ.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }
    }
}