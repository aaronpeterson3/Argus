using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Argus.Core.Infrastructure.FeatureFlags
{
    public class FeatureManager : IFeatureManager
    {
        private readonly IDistributedCache _cache;
        private readonly ICurrentUserService _currentUserService;
        private readonly string _cacheKeyPrefix = "feature:";

        public FeatureManager(
            IDistributedCache cache,
            ICurrentUserService currentUserService)
        {
            _cache = cache;
            _currentUserService = currentUserService;
        }

        public async Task<bool> IsEnabledAsync(string featureName)
        {
            var tenantId = _currentUserService.TenantId;
            return await IsEnabledAsync(featureName, tenantId);
        }

        public async Task<bool> IsEnabledAsync(string featureName, string tenantId)
        {
            var feature = await GetFeatureDefinitionAsync(featureName);
            if (feature == null) return false;

            if (!string.IsNullOrEmpty(tenantId) && 
                feature.TenantOverrides.TryGetValue(tenantId, out bool tenantOverride))
            {
                return tenantOverride;
            }

            return feature.IsEnabled;
        }

        public async Task<T> GetParameterAsync<T>(string featureName, string parameterName, T defaultValue = default)
        {
            var feature = await GetFeatureDefinitionAsync(featureName);
            if (feature == null) return defaultValue;

            if (feature.Parameters.TryGetValue(parameterName, out string value))
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }

        public async Task<IEnumerable<FeatureDefinition>> GetAllFeaturesAsync()
        {
            var features = new List<FeatureDefinition>();
            var cacheKeys = await _cache.GetAsync("feature:keys");
            
            if (cacheKeys != null)
            {
                var featureNames = JsonSerializer.Deserialize<List<string>>(cacheKeys);
                foreach (var name in featureNames)
                {
                    var feature = await GetFeatureDefinitionAsync(name);
                    if (feature != null)
                    {
                        features.Add(feature);
                    }
                }
            }

            return features;
        }

        public async Task UpdateFeatureAsync(string featureName, bool isEnabled)
        {
            var feature = await GetFeatureDefinitionAsync(featureName) ?? new FeatureDefinition
            {
                Name = featureName
            };

            feature.IsEnabled = isEnabled;
            await SaveFeatureDefinitionAsync(feature);
        }

        public async Task UpdateTenantOverrideAsync(string featureName, string tenantId, bool isEnabled)
        {
            var feature = await GetFeatureDefinitionAsync(featureName);
            if (feature == null) return;

            feature.TenantOverrides[tenantId] = isEnabled;
            await SaveFeatureDefinitionAsync(feature);
        }

        private async Task<FeatureDefinition> GetFeatureDefinitionAsync(string featureName)
        {
            var cacheKey = $"{_cacheKeyPrefix}{featureName}";
            var cached = await _cache.GetAsync(cacheKey);
            
            if (cached == null) return null;
            
            return JsonSerializer.Deserialize<FeatureDefinition>(cached);
        }

        private async Task SaveFeatureDefinitionAsync(FeatureDefinition feature)
        {
            var cacheKey = $"{_cacheKeyPrefix}{feature.Name}";
            var serialized = JsonSerializer.SerializeToUtf8Bytes(feature);
            
            await _cache.SetAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });

            // Update feature keys list
            var keysKey = "feature:keys";
            var existingKeys = await _cache.GetAsync(keysKey);
            var featureNames = existingKeys != null 
                ? JsonSerializer.Deserialize<List<string>>(existingKeys)
                : new List<string>();

            if (!featureNames.Contains(feature.Name))
            {
                featureNames.Add(feature.Name);
                await _cache.SetAsync(keysKey, JsonSerializer.SerializeToUtf8Bytes(featureNames));
            }
        }
    }
}