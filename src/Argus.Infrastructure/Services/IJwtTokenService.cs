using System.Collections.Generic;
using System.Threading.Tasks;

namespace Argus.Infrastructure.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(string userId, string email, IEnumerable<string> roles);
    }
}