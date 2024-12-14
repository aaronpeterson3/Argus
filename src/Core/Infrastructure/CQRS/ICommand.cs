using MediatR;

namespace Argus.Core.Infrastructure.CQRS
{
    public interface ICommand<TResponse> : IRequest<Result<TResponse>>
    {
    }

    public interface ICommand : IRequest<Result<Unit>>
    {
    }
}