namespace Argus.Features.Authentication.Domain.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string resetToken);
        Task SendWelcomeEmailAsync(string email, string verificationToken);
    }

    public class EmailService : IEmailService
    {
        public async Task SendPasswordResetEmailAsync(string email, string resetToken)
        {
            // Implementation for sending password reset email
            throw new NotImplementedException();
        }

        public async Task SendWelcomeEmailAsync(string email, string verificationToken)
        {
            // Implementation for sending welcome email
            throw new NotImplementedException();
        }
    }
}