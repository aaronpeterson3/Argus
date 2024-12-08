using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Argus.Infrastructure.Logging
{
    public class MetricsLogger
    {
        private readonly ILogger _logger;

        public MetricsLogger(ILogger<MetricsLogger> logger)
        {
            _logger = logger;
        }

        public IDisposable BeginOperation(string operationName)
        {
            return new OperationTimer(_logger, operationName);
        }

        private class OperationTimer : IDisposable
        {
            private readonly ILogger _logger;
            private readonly string _operationName;
            private readonly Stopwatch _sw;

            public OperationTimer(ILogger logger, string operationName)
            {
                _logger = logger;
                _operationName = operationName;
                _sw = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _sw.Stop();
                _logger.LogInformation(
                    new EventId(LogConstants.EventIds.PerformanceMetric, "OperationMetric"),
                    "{OperationName} completed in {ElapsedMilliseconds}ms",
                    _operationName, _sw.ElapsedMilliseconds);
            }
        }
    }
}