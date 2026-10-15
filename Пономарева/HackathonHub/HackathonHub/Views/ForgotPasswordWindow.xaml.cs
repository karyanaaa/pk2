using System.Windows;
using HackathonHub.Client.ViewModels;

namespace HackathonHub.Client.Views
{
    public partial class ForgotPasswordWindow : Window
    {
        public ForgotPasswordWindow()
        {
            InitializeComponent();
            DataContext = new ForgotPasswordViewModel();
        }

        private void OpenResetPassword_Click(object sender, RoutedEventArgs e)
        {
            var win = new ResetPasswordWindow();
            win.Owner = this;
            win.ShowDialog();
        }
    }
}