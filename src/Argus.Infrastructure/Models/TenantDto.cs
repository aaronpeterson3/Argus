namespace Argus.Infrastructure.Models;

public record TenantDto(
    Guid Id,
    string Name,
    bool IsActive,
    List<string> Permissions
);