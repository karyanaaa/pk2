using System.Windows;
using System.Windows.Controls;
using HackathonHub.Client.ViewModels;

namespace HackathonHub.Client.Views
{
    public partial class ResetPasswordWindow : Window
    {
        public ResetPasswordWindow(string token = null)
        {
            InitializeComponent();
            DataContext = new ResetPasswordViewModel(token);
        }

        private void NewPwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ResetPasswordViewModel vm)
                vm.NewPassword = ((PasswordBox)sender).Password;
        }

        private void ConfirmBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ResetPasswordViewModel vm)
                vm.ConfirmPassword = ((PasswordBox)sender).Password;
        }
    }
}