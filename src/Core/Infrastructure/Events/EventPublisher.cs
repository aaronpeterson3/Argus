using Orleans;
using Microsoft.Extensions.Logging;

namespace Argus.Core.Infrastructure.Events
{
    public class EventPublisher : IEventPublisher
    {
        private readonly IClusterClient _clusterClient;
        private readonly ILogger<EventPublisher> _logger;
        private readonly ICurrentUserService _currentUserService;

        public EventPublisher(
            IClusterClient clusterClient,
            ILogger<EventPublisher> logger,
            ICurrentUserService currentUserService)
        {
            _clusterClient = clusterClient;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            try
            {
                EnrichEvent(@event);

                var eventGrain = _clusterClient.GetGrain<IEventStreamGrain>(0);
                await eventGrain.PublishAsync(@event);

                _logger.LogInformation(
                    "Published event {EventType} for tenant {TenantId}",
                    @event.Type,
                    @event.TenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing event {EventType}",
                    @event.Type);
                throw;
            }
        }

        public async Task PublishManyAsync<TEvent>(IEnumerable<TEvent> events) where TEvent : IEvent
        {
            foreach (var @event in events)
            {
                await PublishAsync(@event);
            }
        }

        private void EnrichEvent<TEvent>(TEvent @event) where TEvent : IEvent
        {
            if (string.IsNullOrEmpty(@event.TenantId))
            {
                @event.TenantId = _currentUserService.TenantId;
            }

            if (string.IsNullOrEmpty(@event.UserId))
            {
                @event.UserId = _currentUserService.UserId;
            }
        }
    }
}