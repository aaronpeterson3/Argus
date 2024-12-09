using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Argus.Abstractions;
using Argus.Infrastructure.Services;
using Argus.Api.Models;

namespace Argus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IClusterClient _client;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IClusterClient client, IJwtTokenService jwtTokenService)
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
        var token = _jwtTokenService.GenerateToken(
            state.Id.ToString(),
            state.Email,
            state.TenantAccess.Select(ta => ta.Role));

        return Ok(new { token, user = state });
    }
}