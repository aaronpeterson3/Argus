using MediatR;
using Microsoft.Extensions.Logging;

namespace Argus.Core.Infrastructure.CQRS
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, Result<TResponse>>
        where TRequest : IRequest<Result<TResponse>>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<Result<TResponse>> Handle(
            TRequest request,
            RequestHandlerDelegate<Result<TResponse>> next,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling {RequestType}",
                typeof(TRequest).Name);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var response = await next();
                sw.Stop();

                _logger.LogInformation(
                    "Handled {RequestType} in {ElapsedMs}ms",
                    typeof(TRequest).Name,
                    sw.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(
                    ex,
                    "Error handling {RequestType} after {ElapsedMs}ms",
                    typeof(TRequest).Name,
                    sw.ElapsedMilliseconds);
                throw;
            }
        }
    }
}