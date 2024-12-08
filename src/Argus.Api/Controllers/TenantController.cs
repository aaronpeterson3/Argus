using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Argus.Infrastructure.MultiTenant;

namespace Argus.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TenantController : ControllerBase
    {
        private readonly IClusterClient _client;

        public TenantController(IClusterClient client)
        {
            _client = client;
        }

        [HttpPost("invite")]
        [Authorize(Policy = "TenantAdmin")]
        public async Task<IActionResult> InviteUser([FromBody] InviteUserRequest request)
        {
            var tenantGrain = _client.GetGrain<ITenantGrain>(request.TenantId);
            var result = await tenantGrain.InviteUser(request.Email, request.Role);

            if (!result)
                return BadRequest("Failed to invite user");

            return Ok();
        }

        [HttpPost("accept-invite")]
        public async Task<IActionResult> AcceptInvite([FromBody] AcceptInviteRequest request)
        {
            var tenantGrain = _client.GetGrain<ITenantGrain>(request.TenantId);
            var result = await tenantGrain.AcceptInvite(GetCurrentUserId(), request.InviteToken);

            if (!result)
                return BadRequest("Invalid or expired invite");

            return Ok();
        }

        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirst("sub")?.Value);
        }
    }

    public record InviteUserRequest(Guid TenantId, string Email, string Role);
    public record AcceptInviteRequest(Guid TenantId, string InviteToken);
}