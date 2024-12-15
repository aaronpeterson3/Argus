using MediatR;

namespace Argus.Features.Authentication.Api.Commands
{
    public record ForgotPasswordCommand(string Email) : IRequest<Result<Unit>>;

    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            // Implementation will be moved here from existing code
            throw new NotImplementedException();
        }
    }
}