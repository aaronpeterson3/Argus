namespace Argus.Api.Models
{
    public record LoginRequest(
        string Email,
        string Password
    );

    public record RegisterRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string DisplayName
    );

    public record PasswordResetRequest(
        string Email
    );

    public record PasswordResetConfirmRequest(
        string Email,
        string Token,
        string NewPassword
    );
}