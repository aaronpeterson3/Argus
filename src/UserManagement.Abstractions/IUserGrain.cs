using System.Threading.Tasks;
using Orleans;

namespace UserManagement.Abstractions
{
    public interface IUserGrain : IGrainWithStringKey
    {
        Task<bool> ValidateCredentialsAsync(string password);
        Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
        Task<UserState> GetStateAsync();
        Task<bool> CreateAsync(string email, string password, UserProfile profile);
    }

    public record UserState
    {
        public string Email { get; init; }
        public UserProfile Profile { get; init; }
        public bool IsActive { get; init; }
    }

    public record UserProfile
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string DisplayName { get; init; }
    }
}