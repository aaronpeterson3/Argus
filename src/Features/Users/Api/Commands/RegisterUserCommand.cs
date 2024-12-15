using MediatR;

namespace Argus.Features.Users.Api.Commands
{
    public record RegisterUserCommand(string Email, string Password, string FirstName, string LastName) : IRequest<Result<string>>;

    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<string>>
    {
        public async Task<Result<string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // Implementation will be moved here from existing code
            throw new NotImplementedException();
        }
    }
}