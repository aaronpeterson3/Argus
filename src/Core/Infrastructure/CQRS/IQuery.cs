using MediatR;

namespace Argus.Core.Infrastructure.CQRS
{
    public interface IQuery<TResponse> : IRequest<Result<TResponse>>
    {
    }
}