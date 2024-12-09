using Argus.Infrastructure.Data.DTOs;

namespace Argus.Infrastructure.Data.Interfaces
{
    public interface IInvitationRepository
    {
        Task CreateAsync(InvitationDto invitation);
        Task<InvitationDto> GetPendingInvitationAsync(string email, string token);
        Task UpdateStatusAsync(Guid id, string status);
    }
}