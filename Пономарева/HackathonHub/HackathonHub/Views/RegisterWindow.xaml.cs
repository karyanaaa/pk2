using System.Windows;
using System.Windows.Controls;
using HackathonHub.Client.ViewModels;

namespace HackathonHub.Client.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
            DataContext = new RegisterViewModel();
        }

        private void PwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm)
                vm.Password = ((PasswordBox)sender).Password;
        }

        private void ConfirmPwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm)
                vm.ConfirmPassword = ((PasswordBox)sender).Password;
        }
    }
}