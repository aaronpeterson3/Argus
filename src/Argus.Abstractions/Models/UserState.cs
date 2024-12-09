namespace Argus.Abstractions.Models
{
    public class UserState
    {
        public string Email { get; set; }
        public UserProfile Profile { get; set; }
        public bool IsActive { get; set; }
    }
}