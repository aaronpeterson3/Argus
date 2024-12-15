namespace Argus.Core.Infrastructure.Events
{
    public interface IEvent
    {
        string Type { get; }
        DateTime Timestamp { get; }
        string TenantId { get; }
        string UserId { get; }
    }

    public abstract class EventBase : IEvent
    {
        public string Type => GetType().Name;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string TenantId { get; set; }
        public string UserId { get; set; }
    }
}