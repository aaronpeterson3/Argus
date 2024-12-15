using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Argus.Core.Infrastructure.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ICurrentUserService _currentUserService;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

        public RedisCacheService(
            IDistributedCache cache,
            ICurrentUserService currentUserService)
        {
            _cache = cache;
            _currentUserService = currentUserService;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var tenantKey = GetTenantKey(key);
            var cached = await _cache.GetAsync(tenantKey);
            
            if (cached == null) return default;
            
            return JsonSerializer.Deserialize<T>(cached);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var tenantKey = GetTenantKey(key);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
            };

            var serialized = JsonSerializer.SerializeToUtf8Bytes(value);
            await _cache.SetAsync(tenantKey, serialized, options);
        }

        public async Task RemoveAsync(string key)
        {
            var tenantKey = GetTenantKey(key);
            await _cache.RemoveAsync(tenantKey);
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            var cached = await GetAsync<T>(key);
            
            if (cached != null) return cached;
            
            var value = await factory();
            await SetAsync(key, value, expiration);
            
            return value;
        }

        private string GetTenantKey(string key)
        {
            var tenantId = _currentUserService.TenantId ?? "global";
            return $"{tenantId}:{key}";
        }
    }
}