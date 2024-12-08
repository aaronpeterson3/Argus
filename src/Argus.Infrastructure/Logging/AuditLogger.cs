using Microsoft.Extensions.Logging;

namespace Argus.Infrastructure.Logging
{
    public class AuditLogger
    {
        private readonly ILogger _logger;

        public AuditLogger(ILogger<AuditLogger> logger)
        {
            _logger = logger;
        }

        public void LogUserAction(string userId, string action, object details = null)
        {
            _logger.LogInformation(
                new EventId(LogConstants.EventIds.AuditEvent, "UserAction"),
                "User {UserId} performed {Action} with details {@Details}",
                userId, action, details);
        }

        public void LogSecurityEvent(string userId, string eventType, string description)
        {
            _logger.LogWarning(
                new EventId(LogConstants.EventIds.SecurityEvent, "SecurityEvent"),
                "Security event {EventType} for user {UserId}: {Description}",
                eventType, userId, description);
        }

        public void LogDataChange(string userId, string entity, string action, object before, object after)
        {
            _logger.LogInformation(
                new EventId(LogConstants.EventIds.DatabaseOperation, "DataChange"),
                "User {UserId} {Action} {Entity}. Before: {@Before}, After: {@After}",
                userId, action, entity, before, after);
        }
    }
}