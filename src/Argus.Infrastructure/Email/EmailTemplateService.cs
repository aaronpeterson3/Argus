using Mjml.Net;

namespace Argus.Infrastructure.Email
{
    public class EmailTemplateService
    {
        private readonly IMjmlRenderer _mjmlRenderer;
        private readonly IWebHostEnvironment _environment;

        public EmailTemplateService(IMjmlRenderer mjmlRenderer, IWebHostEnvironment environment)
        {
            _mjmlRenderer = mjmlRenderer;
            _environment = environment;
        }

        public async Task<string> RenderInvitationEmail(InvitationEmailModel model)
        {
            var templatePath = Path.Combine(_environment.ContentRootPath, "Email/Templates/invitation.mjml");
            var template = await File.ReadAllTextAsync(templatePath);

            var mjmlTemplate = template
                .Replace("{{logoUrl}}", model.LogoUrl)
                .Replace("{{businessName}}", model.BusinessName)
                .Replace("{{inviteeName}}", model.InviteeName)
                .Replace("{{inviterName}}", model.InviterName)
                .Replace("{{inviterEmail}}", model.InviterEmail)
                .Replace("{{inviteUrl}}", model.InviteUrl)
                .Replace("{{currentYear}}", DateTime.UtcNow.Year.ToString());

            var result = _mjmlRenderer.Render(mjmlTemplate);
            return result.Html;
        }
    }

    public record InvitationEmailModel(
        string LogoUrl,
        string BusinessName,
        string InviteeName,
        string InviterName,
        string InviterEmail,
        string InviteUrl
    );
}