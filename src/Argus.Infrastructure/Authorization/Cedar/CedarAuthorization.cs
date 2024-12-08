using Cedar.Core;

namespace Argus.Infrastructure.Authorization.Cedar
{
    public class CedarAuthorization : IAuthorizationService
    {
        private readonly Authorizer _authorizer;
        private readonly ILogger<CedarAuthorization> _logger;

        public CedarAuthorization(ILogger<CedarAuthorization> logger)
        {
            var policies = LoadPolicies();
            _authorizer = new Authorizer(policies);
            _logger = logger;
        }

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

    public record AuthorizationRequest
    {
        public Guid UserId { get; init; }
        public IEnumerable<string> UserRoles { get; init; }
        public Guid TenantId { get; init; }
        public string Action { get; init; }
        public string ResourceType { get; init; }
        public Guid ResourceId { get; init; }
        public Guid ResourceTenantId { get; init; }
        public string Sensitivity { get; init; }
    }
}