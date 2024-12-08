using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Abstractions;

namespace UserManagement.Api.Services
{
    public class JwtTokenService
    {
        private readonly AuthConfig _authConfig;

        public JwtTokenService(IOptions<AuthConfig> authOptions)
        {
            _authConfig = authOptions.Value;
        }

        public string GenerateToken(UserState user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_authConfig.JwtSecret);
            
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.Profile.FirstName),
                new Claim(ClaimTypes.Surname, user.Profile.LastName),
                new Claim("displayName", user.Profile.DisplayName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_authConfig.TokenExpirationMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}