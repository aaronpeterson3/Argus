using MediatR;

namespace Argus.Features.Authentication.Api.Commands
{
    public record LoginCommand(string Email, string Password) : IRequest<Result<AuthenticationResult>>;

    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthenticationResult>>
    {
        public async Task<Result<AuthenticationResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // Implementation will be moved here from existing code
            throw new NotImplementedException();
        }
    }
}