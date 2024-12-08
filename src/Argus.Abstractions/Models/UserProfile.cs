namespace Argus.Abstractions.Models
{
    /// <summary>
    /// Contains user profile information
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// User's display name or username
        /// </summary>
        public string DisplayName { get; set; }
    }
}