using System.Threading.Tasks;
using Orleans;

namespace Argus.Abstractions
{
    public interface IUserGrain : IGrainWithStringKey
    {
        Task<bool> ValidateCredentialsAsync(string password);
        Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<string> RequestPasswordResetAsync(string email);
        Task<UserState> GetStateAsync();
        Task<bool> CreateAsync(string email, string password, UserProfile profile);
    }
}