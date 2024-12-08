using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using UserManagement.Abstractions;
using UserManagement.Api.Services;

namespace UserManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IClusterClient _client;
        private readonly JwtTokenService _jwtTokenService;

        public AuthController(IClusterClient client, JwtTokenService jwtTokenService)
        {
            _client = client;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var userGrain = _client.GetGrain<IUserGrain>(request.Email);
            var isValid = await userGrain.ValidateCredentialsAsync(request.Password);

            if (!isValid)
                return Unauthorized();

            var state = await userGrain.GetStateAsync();
            var token = _jwtTokenService.GenerateToken(state);

            return Ok(new { token, user = state });
        }

        [Authorize]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var userGrain = _client.GetGrain<IUserGrain>(email);
            var state = await userGrain.GetStateAsync();
            
            if (state == null)
                return NotFound();

            var token = _jwtTokenService.GenerateToken(state);
            return Ok(new { token, user = state });
        }
    }
}