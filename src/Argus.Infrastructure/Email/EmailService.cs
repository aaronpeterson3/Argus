namespace Argus.Infrastructure.Email
{
    public class EmailService
    {
        private readonly EmailTemplateService _templateService;
        private readonly ILogger<EmailService> _logger;
        private readonly EmailConfig _config;

        public async Task SendInvitationEmail(InvitationEmailModel model)
        {
            try
            {
                var htmlContent = await _templateService.RenderInvitationEmail(model);
                
                using var message = new MailMessage
                {
                    From = new MailAddress(_config.FromEmail, model.BusinessName),
                    Subject = $"Join {model.BusinessName} on Argus",
                    Body = htmlContent,
                    IsBodyHtml = true
                };
                message.To.Add(model.InviteeName);

                using var client = new SmtpClient(_config.SmtpServer)
                {
                    Port = _config.SmtpPort,
                    Credentials = new NetworkCredential(_config.SmtpUsername, _config.SmtpPassword),
                    EnableSsl = true
                };

                await client.SendMailAsync(message);
                _logger.LogInformation("Sent invitation email to {Email}", model.InviteeName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send invitation email to {Email}", model.InviteeName);
                throw;
            }
        }
    }
}