namespace Argus.Core.Infrastructure.Events
{
    public interface IEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent;
        Task PublishManyAsync<TEvent>(IEnumerable<TEvent> events) where TEvent : IEvent;
    }
}