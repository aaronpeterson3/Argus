using Argus.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Argus.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly AuthConfig _authConfig;
    private static class Claims
    {
        public const string Sub = "sub";
        public const string Email = "email";
        public const string Role = "role";
        public const string Jti = "jti";
    }

    public JwtTokenService(IOptions<AuthConfig> authOptions)
    {
        _authConfig = authOptions.Value;
    }

    public string GenerateToken(string userId, string email, IEnumerable<string> roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_authConfig.JwtSecret);

        var claims = new List<Claim>
        {
            new(Claims.Sub, userId),
            new(Claims.Email, email),
            new(Claims.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(Claims.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_authConfig.TokenExpirationMinutes),
            Issuer = _authConfig.Issuer,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_authConfig.JwtSecret);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = _authConfig.Issuer,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch (SecurityTokenException)
        {
            throw new SecurityTokenException("Invalid token");
        }
    }
}