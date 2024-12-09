namespace Argus.Infrastructure.Configuration;

public sealed class AuthConfig(
    string jwtSecret,
    int tokenExpirationMinutes = 60)
{
    public string JwtSecret { get; init; } = jwtSecret;
    public int TokenExpirationMinutes { get; init; } = tokenExpirationMinutes;
}