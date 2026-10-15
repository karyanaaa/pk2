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
    /// Логика взаимодействия для UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        public UserWindow(string login)
        {
            InitializeComponent();
            TxtWelcome.Text = $"Добро пожаловать, {login}!\nВы вошли как Пользователь.";
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}
