namespace Argus.Infrastructure.Data.DTOs;

public sealed class TenantDto(
    Guid id,
    string name,
    string subdomain,
    string? logoUrl = null)
{
    public Guid Id { get; init; } = id;
    public string Name { get; init; } = name;
    public string Subdomain { get; init; } = subdomain;
    public string? LogoUrl { get; init; } = logoUrl;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object> Settings { get; init; } = [];
}