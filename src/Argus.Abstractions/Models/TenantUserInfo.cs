namespace Argus.Abstractions.Models
{
    public record TenantUserInfo(
        Guid UserId,
        string Role,
        DateTime JoinedAt
    );
}