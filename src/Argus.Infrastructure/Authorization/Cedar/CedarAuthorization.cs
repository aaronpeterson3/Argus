using Cedar.Core;

namespace Argus.Infrastructure.Authorization.Cedar
{
    /// <summary>
    /// Implements Cedar-based authorization for fine-grained access control
    /// </summary>
    public class CedarAuthorization : IAuthorizationService
    {
        private readonly Authorizer _authorizer;
        private readonly ILogger<CedarAuthorization> _logger;

        /// <summary>
        /// Initializes Cedar authorization service with policy definitions
        /// </summary>
        public CedarAuthorization(ILogger<CedarAuthorization> logger)
        {
            var policies = LoadPolicies();
            _authorizer = new Authorizer(policies);
            _logger = logger;
        }

        /// <summary>
        /// Evaluates authorization request against Cedar policies
        /// </summary>
        /// <param name="request">Authorization request details</param>
        /// <returns>True if authorized, false otherwise</returns>
        public async Task<bool> AuthorizeAsync(AuthorizationRequest request)
        {
            try
            {
                var principal = new Entity($"User::{request.UserId}", new Dictionary<string, object>
                {
                    { "roles", request.UserRoles },
                    { "tenant", request.TenantId },
                    { "type", "user" }
                });

                var resource = new Entity($"Resource::{request.ResourceType}::{request.ResourceId}", 
                    new Dictionary<string, object>
                    {
                        { "owner", request.ResourceTenantId },
                        { "type", request.ResourceType.ToLower() },
                        { "sensitivity", request.Sensitivity ?? "low" }
                    });

                var result = await _authorizer.IsAuthorizedAsync(
                    principal,
                    request.Action,
                    resource);

                _logger.LogInformation(
                    "Authorization {Result} for user {UserId} to {Action} {ResourceType}",
                    result ? "granted" : "denied",
                    request.UserId,
                    request.Action,
                    request.ResourceType);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authorization failed");
                return false;
            }
        }

        private PolicySet LoadPolicies()
        {
            var policyJson = File.ReadAllText("Authorization/Cedar/cedar-policies.json");
            return PolicySet.FromJson(policyJson);
        }
    }

    /// <summary>
    /// Represents an authorization request for Cedar evaluation
    /// </summary>
    public record AuthorizationRequest
    {
        /// <summary>
        /// ID of the user requesting access
        /// </summary>
        public Guid UserId { get; init; }

        /// <summary>
        /// Roles assigned to the user
        /// </summary>
        public IEnumerable<string> UserRoles { get; init; }

        /// <summary>
        /// ID of the tenant the user belongs to
        /// </summary>
        public Guid TenantId { get; init; }

        /// <summary>
        /// Action being requested (e.g., "read", "write")
        /// </summary>
        public string Action { get; init; }

        /// <summary>
        /// Type of resource being accessed
        /// </summary>
        public string ResourceType { get; init; }

        /// <summary>
        /// ID of the specific resource
        /// </summary>
        public Guid ResourceId { get; init; }

        /// <summary>
        /// ID of the tenant that owns the resource
        /// </summary>
        public Guid ResourceTenantId { get; init; }

        /// <summary>
        /// Optional sensitivity level of the resource
        /// </summary>
        public string Sensitivity { get; init; }
    }
}