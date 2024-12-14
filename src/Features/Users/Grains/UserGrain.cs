using Orleans;
using Argus.Features.Users.Domain.Models;

namespace Argus.Features.Users.Grains
{
    public interface IUserGrain : IGrainWithStringKey
    {
        Task<UserProfile> GetProfileAsync();
        Task UpdateProfileAsync(UserProfile profile);
    }

    public class UserGrain : Grain, IUserGrain
    {
        private UserProfile _profile;

        public Task<UserProfile> GetProfileAsync()
        {
            return Task.FromResult(_profile);
        }

        public Task UpdateProfileAsync(UserProfile profile)
        {
            _profile = profile;
            return Task.CompletedTask;
        }
    }
}