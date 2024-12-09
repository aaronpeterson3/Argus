namespace Argus.Infrastructure.Data.DTOs;

public sealed class UserDto(
    Guid id,
    string email,
    string firstName,
    string lastName,
    string? displayName = null)
{
    public Guid Id { get; init; } = id;
    public string Email { get; init; } = email;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; init; } = firstName;
    public string LastName { get; init; } = lastName;
    public string? DisplayName { get; init; } = displayName;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}