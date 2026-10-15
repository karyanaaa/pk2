using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using HackathonHub.Client.Models;
using HackathonHub.Client.Services;

namespace HackathonHub.Client.Views
{
    public partial class UploadProjectWindow : Window
    {
        private readonly ApiService _api = ApiService.Instance;
        private readonly DialogService _dialog = new DialogService();
        private readonly int _teamId;

        private string _selectedFilePath;
        private string _selectedPresentationPath;

        public UploadProjectWindow(int teamId)
        {
            InitializeComponent();
            _teamId = teamId;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Выберите архив с проектом",
                Filter = "Архивы (*.zip;*.rar)|*.zip;*.rar|Все файлы (*.*)|*.*"
            };
            if (dlg.ShowDialog() == true)
            {
                _selectedFilePath = dlg.FileName;
                FilePathText.Text = Path.GetFileName(dlg.FileName);
                FilePathText.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void BrowsePresentation_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Выберите презентацию",
                Filter = "Презентации (*.pdf;*.pptx;*.ppt)|*.pdf;*.pptx;*.ppt|Все файлы (*.*)|*.*"
            };
            if (dlg.ShowDialog() == true)
            {
                _selectedPresentationPath = dlg.FileName;
                PresentationPathText.Text = Path.GetFileName(dlg.FileName);
                PresentationPathText.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private async void Upload_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                _dialog.ShowWarning("Введите название проекта");
                return;
            }

            var project = new Project
            {
                TeamId = _teamId,
                Title = TitleBox.Text.Trim(),
                Description = DescBox.Text?.Trim(),
                RepositoryUrl = string.IsNullOrWhiteSpace(RepoBox.Text) ? null : RepoBox.Text.Trim(),
                FilePath = _selectedFilePath,             // локальный путь к архиву
                PresentationPath = _selectedPresentationPath
            };

            try
            {
                var resp = await _api.UploadProjectAsync(project);
                if (resp.Success)
                {
                    _dialog.ShowInfo("Проект загружен!");
                    DialogResult = true;
                    Close();
                }
                else
                {
                    _dialog.ShowError(resp.Message ?? "Ошибка загрузки");
                }
            }
            catch (Exception ex)
            {
                _dialog.ShowError(ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}