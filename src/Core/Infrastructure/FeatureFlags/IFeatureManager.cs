namespace Argus.Core.Infrastructure.FeatureFlags
{
    public interface IFeatureManager
    {
        Task<bool> IsEnabledAsync(string featureName);
        Task<bool> IsEnabledAsync(string featureName, string tenantId);
        Task<T> GetParameterAsync<T>(string featureName, string parameterName, T defaultValue = default);
        Task<IEnumerable<FeatureDefinition>> GetAllFeaturesAsync();
        Task UpdateFeatureAsync(string featureName, bool isEnabled);
        Task UpdateTenantOverrideAsync(string featureName, string tenantId, bool isEnabled);
    }
}