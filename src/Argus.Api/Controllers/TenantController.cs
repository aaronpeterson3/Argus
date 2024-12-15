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
        /// Initializes the system by creating the first tenant. Only works when no tenants exist.
        /// </summary>
        /// <param name="request">The tenant creation request</param>
        /// <returns>Success if the tenant was created</returns>
        /// <response code="200">The first tenant was created successfully</response>
        /// <response code="400">System is already initialized or the request is invalid</response>
        [HttpPost("initialize")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateFirstTenant([FromBody] CreateFirstTenantRequest request)
        {
            // Check if any tenants exist
            var tenantGrain = _client.GetGrain<ITenantGrain>(request.TenantId);
            var existingState = await tenantGrain.GetStateAsync();
            
            if (existingState != null)
                return BadRequest("System is already initialized");

            // Create the first tenant
            var result = await tenantGrain.UpdateAsync(new TenantState
            {
                Id = request.TenantId,
                Name = request.Name,
                OwnerId = request.OwnerId,
                CreatedAt = DateTime.UtcNow
            });

            if (!result)
                return BadRequest("Failed to create tenant");

            return Ok();
        }

        /// <summary>
        /// Invites a user to join a tenant
        /// </summary>
        /// <param name="request">The invite request containing tenant ID, email, and role</param>
        /// <returns>Success if the invite was created</returns>
        [HttpPost("invite")]
        [Authorize(Policy = "TenantAdmin")]
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
        [HttpPost("accept-invite")]
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
    /// Request model for creating the first tenant
    /// </summary>
    /// <param name="TenantId">The ID for the new tenant</param>
    /// <param name="Name">The name of the tenant</param>
    /// <param name="OwnerId">The ID of the tenant owner</param>
    public record CreateFirstTenantRequest(Guid TenantId, string Name, Guid OwnerId);

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