using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;

namespace Argus.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailConfig _config;

    public EmailService(ILogger<EmailService> logger, EmailConfig config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_config.FromAddress),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(to);

        using var client = new SmtpClient(_config.SmtpServer, _config.SmtpPort)
        {
            Credentials = new NetworkCredential(_config.Username, _config.Password),
            EnableSsl = true
        };

        await client.SendMailAsync(message);
        _logger.LogInformation("Email sent to {To}", to);
    }
}

public record EmailConfig(
    string FromAddress,
    string SmtpServer,
    int SmtpPort,
    string Username,
    string Password
);