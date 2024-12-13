namespace Argus.Infrastructure.Email
{
    public class EmailConfig
    {
        public required string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public required string SmtpUsername { get; set; }
        public required string SmtpPassword { get; set; }
        public required string FromEmail { get; set; }
        public required string FromName { get; set; }
    }
}