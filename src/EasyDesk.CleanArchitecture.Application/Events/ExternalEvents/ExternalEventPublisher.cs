using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Application.Tenants;
using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.ExternalEvents
{
    public class ExternalEventPublisher : IExternalEventPublisher
    {
        private readonly IEventBusPublisher _eventBusPublisher;
        private readonly ITimestampProvider _timestampProvider;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ITenantProvider _tenantProvider;

        public ExternalEventPublisher(
            IEventBusPublisher eventBusPublisher,
            ITimestampProvider timestampProvider,
            IJsonSerializer jsonSerializer,
            ITenantProvider tenantProvider)
        {
            _eventBusPublisher = eventBusPublisher;
            _timestampProvider = timestampProvider;
            _jsonSerializer = jsonSerializer;
            _tenantProvider = tenantProvider;
        }

        public async Task Publish(IEnumerable<ExternalEvent> events)
        {
            var eventBusMessages = events.Select(e => ToEventBusMessage(e)).ToList();
            await _eventBusPublisher.Publish(eventBusMessages);
        }

        private EventBusMessage ToEventBusMessage(ExternalEvent externalEvent)
        {
            return new EventBusMessage(
                Id: Guid.NewGuid(),
                OccurredAt: _timestampProvider.Now,
                EventType: externalEvent.GetType().GetEventTypeName(),
                TenantId: _tenantProvider.TenantId,
                Content: _jsonSerializer.Serialize(externalEvent));
        }
    }
}
