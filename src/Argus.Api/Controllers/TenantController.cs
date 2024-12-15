using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Argus.Infrastructure.MultiTenant;
using Argus.Abstractions.Grains;

namespace Argus.Api.Controllers
{
    /// <summary>
    /// Controller for managing tenant operations
    /// </summary>
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

        /// <summary>
        /// Invites a user to join a tenant
        /// </summary>
        /// <param name="request">The invite request containing tenant ID, email, and role</param>
        /// <returns>Success if the invite was created</returns>
        /// <response code="200">The invite was created successfully</response>
        /// <response code="400">The invite could not be created</response>
        /// <response code="403">User does not have tenant admin permissions</response>
        [HttpPost("invite")]
        [Authorize(Policy = "TenantAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> InviteUser([FromBody] InviteUserRequest request)
        {
            var tenantGrain = _client.GetGrain<ITenantGrain>(request.TenantId);
            var result = await tenantGrain.InviteUserAsync(request.Email, request.Role, GetCurrentUserId());

            if (string.IsNullOrEmpty(result))
            {
                return BadRequest("Failed to invite user");
            }
            return Ok();
        }

        /// <summary>
        /// Accepts a tenant invite
        /// </summary>
        /// <param name="request">The accept invite request containing tenant ID and invite token</param>
        /// <returns>Success if the invite was accepted</returns>
        /// <response code="200">The invite was accepted successfully</response>
        /// <response code="400">The invite was invalid or expired</response>
        [HttpPost("accept-invite")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AcceptInvite([FromBody] AcceptInviteRequest request)
        {
            var userEmail = User.FindFirst("email")?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return BadRequest("User email not found in claims");

            var tenantGrain = _client.GetGrain<ITenantGrain>(request.TenantId);
            var result = await tenantGrain.AcceptInviteAsync(userEmail, request.InviteToken);

            if (!result)
                return BadRequest("Invalid or expired invite");

            return Ok();
        }

        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirst("sub")?.Value);
        }
    }

    /// <summary>
    /// Request model for inviting a user to a tenant
    /// </summary>
    /// <param name="TenantId">The ID of the tenant</param>
    /// <param name="Email">The email of the user to invite</param>
    /// <param name="Role">The role to assign to the user</param>
    public record InviteUserRequest(Guid TenantId, string Email, string Role);

    /// <summary>
    /// Request model for accepting a tenant invite
    /// </summary>
    /// <param name="TenantId">The ID of the tenant</param>
    /// <param name="InviteToken">The invite token received via email</param>
    public record AcceptInviteRequest(Guid TenantId, string InviteToken);
}