using System.Net;
using System.Net.Mail;

namespace EmailServer.Services;

public static class EmailService
{
    public static async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var cfg = EmailConfig.Instance;

        using var message = new MailMessage(
            from: new MailAddress(cfg.FromAddr, cfg.FromName),
            to: new MailAddress(toEmail))
        {
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        // Настраиваем SMTP-клиент
        using var smtpClient = new SmtpClient(cfg.SmtpHost, cfg.SmtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(cfg.SmtpUser, cfg.SmtpPass)
        };

        // Отправляем письмо
        await smtpClient.SendMailAsync(message);
    }
}