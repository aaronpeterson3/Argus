using MediatR;
using Argus.Core.Common;

namespace Argus.Features.Tenants.Api.Commands
{
    public record CreateTenantCommand(
        string Name,
        string OwnerId
    ) : IRequest<Result<string>>;

    public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<string>>
    {
        public async Task<Result<string>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
        {
            // Implementation will be moved here from existing code
            throw new NotImplementedException();
        }
    }
}