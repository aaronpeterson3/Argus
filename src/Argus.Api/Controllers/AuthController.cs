using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Argus.Abstractions;
using Argus.Api.Services;

namespace Argus.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IClusterClient _client;
        private readonly JwtTokenService _jwtTokenService;
        private readonly EmailService _emailService;

        public AuthController(IClusterClient client, JwtTokenService jwtTokenService, EmailService emailService)
        {
            _client = client;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] ResetPasswordRequest request)
        {
            var userGrain = _client.GetGrain<IUserGrain>(request.Email);
            var token = await userGrain.RequestPasswordResetAsync(request.Email);

            if (string.IsNullOrEmpty(token))
                return NotFound("User not found");

            await _emailService.SendPasswordResetEmailAsync(
                request.Email,
                token);

            return Ok();
        }

        [HttpPost("reset-password/confirm")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordConfirmRequest request)
        {
            var userGrain = _client.GetGrain<IUserGrain>(request.Email);
            var success = await userGrain.ResetPasswordAsync(request.Token, request.NewPassword);

            if (!success)
                return BadRequest("Invalid or expired token");

            return Ok();
        }
    }

    public record ResetPasswordRequest(string Email);
    public record ResetPasswordConfirmRequest(string Email, string Token, string NewPassword);
}