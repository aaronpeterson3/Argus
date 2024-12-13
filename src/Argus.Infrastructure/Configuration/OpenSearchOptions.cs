using System;

namespace Argus.Infrastructure.Configuration
{
    public class OpenSearchOptions
    {
        public Uri Url { get; set; }
        public string IndexFormat { get; set; } = "logs-{0:yyyy.MM}";
        public int BatchSize { get; set; } = 50;
        public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(2);
    }
}