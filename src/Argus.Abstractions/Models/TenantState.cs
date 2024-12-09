namespace Argus.Abstractions.Models
{
    public record TenantState
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string Subdomain { get; init; }
        public string LogoUrl { get; init; }
        public DateTime CreatedAt { get; init; }
        public Dictionary<string, string> Settings { get; init; } = new();
    }
}