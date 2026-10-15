using System.Windows;

namespace HackathonHub.Client
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Обработка deep-link для сброса пароля:
            // hackathonhub://reset-password?token=...
            // Здесь можно реализовать регистрацию протокола
        }
    }
}