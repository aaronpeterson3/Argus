using Orleans;
using Argus.Features.Tenants.Domain.Models;

namespace Argus.Features.Tenants.Grains
{
    public interface ITenantGrain : IGrainWithStringKey
    {
        Task<TenantState> GetStateAsync();
        Task UpdateStateAsync(TenantState state);
        Task<bool> InviteUserAsync(string email, string role);
        Task<bool> AcceptInviteAsync(string token, string userId);
    }

    public class TenantGrain : Grain, ITenantGrain
    {
        private TenantState _state;

        public Task<TenantState> GetStateAsync()
        {
            return Task.FromResult(_state);
        }

        public Task UpdateStateAsync(TenantState state)
        {
            _state = state;
            return Task.CompletedTask;
        }

        public Task<bool> InviteUserAsync(string email, string role)
        {
            // Implementation will be moved here from existing code
            throw new NotImplementedException();
        }

        public Task<bool> AcceptInviteAsync(string token, string userId)
        {
            // Implementation will be moved here from existing code
            throw new NotImplementedException();
        }
    }
}