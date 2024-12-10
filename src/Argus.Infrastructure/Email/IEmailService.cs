namespace Argus.Infrastructure.Email;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}