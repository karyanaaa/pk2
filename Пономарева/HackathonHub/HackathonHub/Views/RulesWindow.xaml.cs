using System.Windows;

namespace HackathonHub.Client.Views
{
    public partial class RulesWindow : Window
    {
        public RulesWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}