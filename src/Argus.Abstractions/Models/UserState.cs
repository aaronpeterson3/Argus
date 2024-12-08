namespace Argus.Abstractions.Models
{
    /// <summary>
    /// Represents the complete state of a user
    /// </summary>
    public class UserState
    {
        /// <summary>
        /// User's email address (unique identifier)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// User's display name or preferred name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Indicates if the user account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// User's profile information
        /// </summary>
        public UserProfile Profile { get; set; }
    }
}