using Argus.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Argus.Infrastructure.Services;

public interface IJwtTokenService
{
    Task<TokenResponse> GenerateTokensAsync(string userId, string email, IEnumerable<string> roles);
    Task<TokenResponse> RefreshTokenAsync(string refreshToken);
    ClaimsPrincipal ValidateToken(string token);
}

public record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);

public class JwtTokenService : IJwtTokenService
{
    private readonly AuthConfig _authConfig;
    private static class Claims
    {
        public const string Sub = "sub";
        public const string Email = "email";
        public const string Role = "role";
        public const string Jti = "jti";
        public const string RefreshTokenId = "refresh_token_id";
    }

    public JwtTokenService(IOptions<AuthConfig> authOptions)
    {
        _authConfig = authOptions.Value;
    }

    public async Task<TokenResponse> GenerateTokensAsync(string userId, string email, IEnumerable<string> roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_authConfig.JwtSecret);
        var refreshTokenId = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(Claims.Sub, userId),
            new(Claims.Email, email),
            new(Claims.Jti, Guid.NewGuid().ToString()),
            new(Claims.RefreshTokenId, refreshTokenId)
        };

        claims.AddRange(roles.Select(role => new Claim(Claims.Role, role)));

        var expires = DateTime.UtcNow.AddMinutes(_authConfig.TokenExpirationMinutes);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _authConfig.Issuer,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var refreshToken = await GenerateRefreshTokenAsync(userId, refreshTokenId);

        return new TokenResponse(
            tokenHandler.WriteToken(token),
            refreshToken,
            expires
        );
    }

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var principal = ValidateToken(refreshToken);
        var userId = principal.FindFirst(Claims.Sub)?.Value;
        var email = principal.FindFirst(Claims.Email)?.Value;
        var roles = principal.FindAll(Claims.Role).Select(c => c.Value);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            throw new SecurityTokenException("Invalid refresh token");
        }

        return await GenerateTokensAsync(userId, email, roles);
    }

    private async Task<string> GenerateRefreshTokenAsync(string userId, string tokenId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_authConfig.JwtSecret);

        var claims = new List<Claim>
        {
            new(Claims.Sub, userId),
            new(Claims.Jti, tokenId)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(_authConfig.RefreshTokenExpirationDays),
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