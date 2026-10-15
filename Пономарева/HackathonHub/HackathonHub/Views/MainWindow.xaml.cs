using System.Windows;
using System.Windows.Controls;
using HackathonHub.Client.Models;
using HackathonHub.Client.ViewModels;

namespace HackathonHub.Client.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void TeamsGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm && sender is DataGrid grid && grid.SelectedItem is TeamOverview team)
            {
                vm.ViewTeamDetailsCommand.Execute(team);
            }
        }

        private void ProjectsGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm && sender is DataGrid grid && grid.SelectedItem is Project p)
            {
                vm.ViewProjectDetailsCommand.Execute(p);
            }
        }

        private async void AcceptRequest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int invId && DataContext is MainViewModel vm)
            {
                await vm.AcceptRequestByIdAsync(invId);
            }
        }

        private async void RejectRequest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int invId && DataContext is MainViewModel vm)
            {
                await vm.RejectRequestByIdAsync(invId);
            }
        }
    }
}