namespace Argus.Core.Infrastructure.FeatureFlags
{
    public class FeatureDefinition
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new();
        
        // Tenant-specific overrides
        public Dictionary<string, bool> TenantOverrides { get; set; } = new();
    }

    public static class FeatureFlags
    {
        public const string BetaFeatures = "beta-features";
        public const string EnhancedSecurity = "enhanced-security";
        public const string NewUserInterface = "new-ui";
        public const string AdvancedAnalytics = "advanced-analytics";
    }
}