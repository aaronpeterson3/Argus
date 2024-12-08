using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Argus.Api.Services
{
    public class EmailService
    {
        private readonly EmailConfig _config;

        public EmailService(IOptions<EmailConfig> options)
        {
            _config = options.Value;
        }

        public async Task SendPasswordResetEmailAsync(string email, string token)
        {
            var resetLink = $"{_config.WebBaseUrl}/reset-password/confirm?token={token}&email={Uri.EscapeDataString(email)}";

            var message = new MailMessage
            {
                From = new MailAddress(_config.FromEmail),
                Subject = "Reset Your Password",
                Body = $"Click the following link to reset your password: {resetLink}",
                IsBodyHtml = true
            };
            message.To.Add(email);

            using var client = new SmtpClient(_config.SmtpServer, _config.SmtpPort)
            {
                EnableSsl = true,
                Credentials = new System.Net.NetworkCredential(_config.SmtpUsername, _config.SmtpPassword)
            };

            await client.SendMailAsync(message);
        }
    }

    public class EmailConfig
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string FromEmail { get; set; }
        public string WebBaseUrl { get; set; }
    }
}