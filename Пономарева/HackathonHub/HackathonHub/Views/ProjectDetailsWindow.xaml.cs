using System;
using System.Diagnostics;
using System.Windows;
using HackathonHub.Client.Models;

namespace HackathonHub.Client.Views
{
    public partial class ProjectDetailsWindow : Window
    {
        private readonly Project _project;

        public ProjectDetailsWindow(Project project)
        {
            InitializeComponent();
            _project = project;

            if (project == null) return;

            TitleText.Text = project.Title ?? "Проект без названия";
            TeamInfoText.Text = $"👥 Команда: {project.TeamName ?? "—"}    🏆 Хакатон: {project.HackathonTitle ?? "—"}";
            DescText.Text = string.IsNullOrWhiteSpace(project.Description)
                ? "Описание отсутствует" : project.Description;

            RepoText.Text = string.IsNullOrWhiteSpace(project.RepositoryUrl)
                ? "Ссылка не указана" : project.RepositoryUrl;

            FilePathText.Text = string.IsNullOrWhiteSpace(project.FilePath)
                ? "Файл не загружен" : project.FilePath;

            PresentationText.Text = string.IsNullOrWhiteSpace(project.PresentationPath)
                ? "Презентация не загружена" : project.PresentationPath;

            UploadedText.Text = $"📅 Загружен: {project.UploadedAt:dd.MM.yyyy HH:mm}";
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenPath(_project?.FilePath, "Файл проекта");
        }

        private void OpenPresentation_Click(object sender, RoutedEventArgs e)
        {
            OpenPath(_project?.PresentationPath, "Презентация");
        }

        private void OpenRepo_Click(object sender, RoutedEventArgs e)
        {
            var url = _project?.RepositoryUrl;
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Ссылка на репозиторий не указана",
                    "Нет ссылки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть ссылку: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenPath(string path, string label)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show($"{label} не указан(а)",
                    "Нет данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (path.StartsWith("http"))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось открыть: " + ex.Message);
                }
                return;
            }

            if (System.IO.File.Exists(path))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось открыть файл: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show($"Файл не найден на этом компьютере:\n{path}\n\n" +
                    "Скорее всего он был загружен на другом ПК.",
                    "Файл не найден", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}