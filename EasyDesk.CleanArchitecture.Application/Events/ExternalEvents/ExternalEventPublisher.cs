using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Domain.Time;
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

        public ExternalEventPublisher(
            IEventBusPublisher eventBusPublisher,
            ITimestampProvider timestampProvider,
            IJsonSerializer jsonSerializer)
        {
            _eventBusPublisher = eventBusPublisher;
            _timestampProvider = timestampProvider;
            _jsonSerializer = jsonSerializer;
        }

        public async Task Publish(IEnumerable<IExternalEvent> events)
        {
            var eventBusMessages = events.Select(ToEventBusMessage).ToList();
            await _eventBusPublisher.Publish(eventBusMessages);
        }

        private EventBusMessage ToEventBusMessage(IExternalEvent externalEvent)
        {
            return new EventBusMessage(
                Id: Guid.NewGuid(),
                EventType: externalEvent.GetType().GetEventTypeName(),
                Content: _jsonSerializer.Serialize(externalEvent),
                OccurredAt: _timestampProvider.Now);
        }
    }
}
