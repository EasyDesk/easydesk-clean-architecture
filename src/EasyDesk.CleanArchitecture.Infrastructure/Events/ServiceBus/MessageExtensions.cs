using Azure.Messaging.ServiceBus;
using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using System;

namespace EasyDesk.CleanArchitecture.Infrastructure.Events
{
    public static class MessageExtensions
    {
        public static ServiceBusMessage ToServiceBusMessage(this EventBusMessage eventBusMessage)
        {
            var serviceBusMessage = new ServiceBusMessage(eventBusMessage.Content)
            {
                ContentType = "application/json",
                MessageId = eventBusMessage.Id.ToString()
            };
            serviceBusMessage.ApplicationProperties.Add(PropertyNames.EventType, eventBusMessage.EventType);
            serviceBusMessage.ApplicationProperties.Add(PropertyNames.OccurredAt, eventBusMessage.OccurredAt.ToString());
            return serviceBusMessage;
        }

        public static EventBusMessage ToEventBusMessage(this ServiceBusReceivedMessage serviceBusMessage)
        {
            return new(
                Id: Guid.Parse(serviceBusMessage.MessageId),
                EventType: serviceBusMessage.ApplicationProperties[PropertyNames.EventType] as string,
                Content: serviceBusMessage.Body.ToString(),
                OccurredAt: Timestamp.Parse(serviceBusMessage.ApplicationProperties[PropertyNames.OccurredAt] as string));
        }
    }
}
