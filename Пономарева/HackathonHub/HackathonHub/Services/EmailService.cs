using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HackathonHub.Client.Services
{
    public class EmailService
    {
        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "hackathonhub.noreply@gmail.com";
        private const string SenderPassword = "your-app-password"; // App password
        private const string SenderName = "HackathonHub";

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            try
            {
                using var client = new SmtpClient(SmtpHost, SmtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(SenderEmail, SenderPassword)
                };

                var resetLink = $"hackathonhub://reset-password?token={resetToken}";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; background: #1a1a2e; padding: 30px;'>
                        <div style='max-width: 600px; margin: 0 auto; background: #16213e; border-radius: 15px; padding: 40px; color: #fff;'>
                            <h1 style='color: #00d9ff; text-align: center;'>HackathonHub</h1>
                            <h2 style='text-align: center;'>Восстановление пароля</h2>
                            <p>Здравствуйте!</p>
                            <p>Вы запросили сброс пароля. Для установки нового пароля нажмите на кнопку ниже:</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{resetLink}' 
                                   style='background: linear-gradient(135deg, #00d9ff, #7b2ff7); 
                                          color: white; padding: 15px 40px; border-radius: 30px;
                                          text-decoration: none; font-weight: bold;'>
                                    Сбросить пароль
                                </a>
                            </div>
                            <p style='color: #888; font-size: 12px;'>
                                Ссылка действительна в течение 1 часа.<br>
                                Если вы не запрашивали сброс, проигнорируйте это письмо.
                            </p>
                            <hr style='border-color: #333;'>
                            <p style='color: #666; font-size: 11px; text-align: center;'>
                                © 2026 HackathonHub. Все права защищены.
                            </p>
                        </div>
                    </body>
                    </html>";

                var message = new MailMessage
                {
                    From = new MailAddress(SenderEmail, SenderName),
                    Subject = "Сброс пароля HackathonHub",
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка отправки письма: {ex.Message}");
            }
        }
    }
}