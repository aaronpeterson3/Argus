using System.Threading.Tasks;
using Orleans;

namespace Argus.Abstractions
{
    /// <summary>
    /// Represents a grain for managing user authentication and profile data
    /// </summary>
    public interface IUserGrain : IGrainWithStringKey
    {
        /// <summary>
        /// Validates user credentials against stored password hash
        /// </summary>
        /// <param name="password">The password to validate</param>
        /// <returns>True if credentials are valid, false otherwise</returns>
        Task<bool> ValidateCredentialsAsync(string password);

        /// <summary>
        /// Changes the user's password after validating current password
        /// </summary>
        /// <param name="currentPassword">Current password for validation</param>
        /// <param name="newPassword">New password to set</param>
        /// <returns>True if password was changed successfully, false otherwise</returns>
        Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);

        /// <summary>
        /// Retrieves the current user state
        /// </summary>
        /// <returns>Current user state or null if user doesn't exist</returns>
        Task<UserState> GetStateAsync();

        /// <summary>
        /// Creates a new user with the specified credentials and profile
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="password">User's password</param>
        /// <param name="profile">User's profile information</param>
        /// <returns>True if user was created successfully, false if user already exists</returns>
        Task<bool> CreateAsync(string email, string password, UserProfile profile);
    }

    /// <summary>
    /// Represents the current state of a user
    /// </summary>
    public record UserState
    {
        /// <summary>
        /// User's email address (unique identifier)
        /// </summary>
        public string Email { get; init; }

        /// <summary>
        /// User's profile information
        /// </summary>
        public UserProfile Profile { get; init; }

        /// <summary>
        /// Indicates if the user account is active
        /// </summary>
        public bool IsActive { get; init; }
    }

    /// <summary>
    /// Contains user profile information
    /// </summary>
    public record UserProfile
    {
        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; init; }

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; init; }

        /// <summary>
        /// User's display name or username
        /// </summary>
        public string DisplayName { get; init; }
    }
}