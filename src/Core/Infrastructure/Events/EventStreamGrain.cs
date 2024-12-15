using Orleans;
using Orleans.Streams;

namespace Argus.Core.Infrastructure.Events
{
    public interface IEventStreamGrain : IGrainWithIntegerKey
    {
        Task PublishAsync(IEvent @event);
        Task<IEnumerable<IEvent>> GetEventsAsync(string tenantId, DateTime? since = null);
    }

    public class EventStreamGrain : Grain, IEventStreamGrain
    {
        private IAsyncStream<IEvent> _stream;
        private readonly IPersistentState<EventStore> _store;

        public EventStreamGrain(
            [PersistentState("events")] IPersistentState<EventStore> store)
        {
            _store = store;
        }

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = GetStreamProvider("EventStream");
            _stream = streamProvider.GetStream<IEvent>(Guid.NewGuid(), "events");
            
            return base.OnActivateAsync(cancellationToken);
        }

        public async Task PublishAsync(IEvent @event)
        {
            await _stream.OnNextAsync(@event);

            _store.State.Events.Add(@event);
            await _store.WriteStateAsync();
        }

        public Task<IEnumerable<IEvent>> GetEventsAsync(string tenantId, DateTime? since = null)
        {
            var events = _store.State.Events
                .Where(e => e.TenantId == tenantId);

            if (since.HasValue)
            {
                events = events.Where(e => e.Timestamp >= since.Value);
            }

            return Task.FromResult(events);
        }
    }

    public class EventStore
    {
        public List<IEvent> Events { get; set; } = new();
    }
}