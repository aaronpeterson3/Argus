using MediatR;
using Argus.Core.Common;

namespace Argus.Features.Tenants.Api.Commands
{
    public record InviteUserCommand(
        string TenantId,
        string Email,
        string Role
    ) : IRequest<Result<Unit>>;

    public class InviteUserCommandHandler : IRequestHandler<InviteUserCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(InviteUserCommand request, CancellationToken cancellationToken)
        {
            // Implementation will be moved here from existing code
            throw new NotImplementedException();
        }
    }
}