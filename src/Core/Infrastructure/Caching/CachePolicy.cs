using System.Text.Json;

namespace Argus.Core.Infrastructure.Caching
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CachePolicyAttribute : Attribute
    {
        public string CacheKeyPrefix { get; }
        public int ExpirationMinutes { get; }
        public bool IncludeTenantId { get; }

        public CachePolicyAttribute(
            string cacheKeyPrefix,
            int expirationMinutes = 30,
            bool includeTenantId = true)
        {
            CacheKeyPrefix = cacheKeyPrefix;
            ExpirationMinutes = expirationMinutes;
            IncludeTenantId = includeTenantId;
        }

        public string GetCacheKey(object[] methodParameters)
        {
            var keyParams = methodParameters
                .Select(p => p?.ToString() ?? "null")
                .ToList();

            return $"{CacheKeyPrefix}:{string.Join(":", keyParams)}";
        }
    }
}