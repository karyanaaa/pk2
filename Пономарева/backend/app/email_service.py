import aiosmtplib
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart
import os
from dotenv import load_dotenv

load_dotenv()

SMTP_HOST = os.getenv("SMTP_HOST", "smtp.gmail.com")
SMTP_PORT = int(os.getenv("SMTP_PORT", 587))
SMTP_USER = os.getenv("SMTP_USER")
SMTP_PASSWORD = os.getenv("SMTP_PASSWORD")
FRONTEND_URL = os.getenv("FRONTEND_URL", "hackathonhub://reset-password")

async def send_reset_email(to_email: str, token: str, user_name: str):
    reset_link = f"{FRONTEND_URL}?token={token}"
    
    html = f"""
    <html>
    <body style="font-family: Arial, sans-serif; background: #1a1a2e; padding: 30px;">
        <div style="max-width: 600px; margin: 0 auto; background: #16213e; 
                    border-radius: 15px; padding: 40px; color: #fff;">
            <h1 style="color: #00d9ff; text-align: center;">HackathonHub</h1>
            <h2 style="text-align: center;">Восстановление пароля</h2>
            <p>Здравствуйте, {user_name}!</p>
            <p>Вы запросили сброс пароля. Для установки нового пароля нажмите на кнопку ниже:</p>
            <div style="text-align: center; margin: 30px 0;">
                <a href="{reset_link}" 
                   style="background: linear-gradient(135deg, #00d9ff, #7b2ff7); 
                          color: white; padding: 15px 40px; border-radius: 30px;
                          text-decoration: none; font-weight: bold; display: inline-block;">
                    Сбросить пароль
                </a>
            </div>
            <p style="color: #888; font-size: 12px;">
                Или скопируйте ссылку:<br>
                <code style="color: #00d9ff; word-break: break-all;">{reset_link}</code>
            </p>
            <p style="color: #888; font-size: 12px;">
                Ссылка действительна в течение 1 часа.<br>
                Если вы не запрашивали сброс, проигнорируйте это письмо.
            </p>
            <hr style="border-color: #333;">
            <p style="color: #666; font-size: 11px; text-align: center;">
                © 2026 HackathonHub. Все права защищены.
            </p>
        </div>
    </body>
    </html>
    """
    
    message = MIMEMultipart("alternative")
    message["From"] = SMTP_USER
    message["To"] = to_email
    message["Subject"] = "Сброс пароля HackathonHub"
    message.attach(MIMEText(html, "html", "utf-8"))
    
    await aiosmtplib.send(
        message,
        hostname=SMTP_HOST,
        port=SMTP_PORT,
        username=SMTP_USER,
        password=SMTP_PASSWORD,
        start_tls=True,
    )