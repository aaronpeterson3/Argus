namespace Argus.Infrastructure.Configuration;

public class AuthConfig
{
    public string JwtSecret { get; set; } = string.Empty;
    public int TokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
    public string Issuer { get; set; } = string.Empty;
    public bool ValidateIssuer { get; set; } = true;
}