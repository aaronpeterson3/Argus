namespace Argus.Infrastructure.Logging
{
    public static class LogConstants
    {
        public static class EventIds
        {
            public const int UserCreated = 1000;
            public const int UserAuthenticated = 1001;
            public const int UserAuthenticationFailed = 1002;
            public const int PasswordChanged = 1003;
            public const int UserProfileUpdated = 1004;
            public const int SecurityEvent = 2000;
            public const int DatabaseOperation = 3000;
            public const int PerformanceMetric = 4000;
            public const int AuditEvent = 5000;
        }

        public static class Categories
        {
            public const string Security = "Security";
            public const string Authentication = "Authentication";
            public const string Database = "Database";
            public const string Performance = "Performance";
            public const string Audit = "Audit";
            public const string Orleans = "Orleans";
        }
    }
}