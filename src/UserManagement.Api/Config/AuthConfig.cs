namespace UserManagement.Api;

public class AuthConfig
{
    public string JwtSecret { get; set; } = string.Empty;
    public int TokenExpirationMinutes { get; set; } = 60;
}