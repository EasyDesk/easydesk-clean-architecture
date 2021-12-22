using Azure.Messaging.ServiceBus;
using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using EasyDesk.Tools.Options;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using System;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus
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
            eventBusMessage.TenantId.IfPresent(tenant =>
            {
                serviceBusMessage.ApplicationProperties.Add(PropertyNames.TenantId, tenant);
            });
            return serviceBusMessage;
        }

        public static EventBusMessage ToEventBusMessage(this ServiceBusReceivedMessage serviceBusMessage)
        {
            var tenantId = serviceBusMessage.ApplicationProperties.TryGetValue(PropertyNames.TenantId, out var tenant)
                ? Some(tenant as string)
                : None;
            return new(
                Id: Guid.Parse(serviceBusMessage.MessageId),
                OccurredAt: Timestamp.Parse(serviceBusMessage.ApplicationProperties[PropertyNames.OccurredAt] as string),
                EventType: serviceBusMessage.ApplicationProperties[PropertyNames.EventType] as string,
                TenantId: tenantId,
                Content: serviceBusMessage.Body.ToString());
        }
    }
}
