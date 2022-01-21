using Azure.Messaging.ServiceBus;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.Tools.Options;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using System;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Messaging.ServiceBus.Utils;

public static class MessageExtensions
{
    public static ServiceBusMessage ToServiceBusMessage(this Message message)
    {
        var serviceBusMessage = new ServiceBusMessage(message.Content)
        {
            ContentType = "application/json",
            MessageId = message.Id.ToString()
        };
        serviceBusMessage.ApplicationProperties.Add(PropertyNames.MessageType, message.Type);
        serviceBusMessage.ApplicationProperties.Add(PropertyNames.MessageTimestamp, message.Timestamp.ToString());
        message.TenantId.IfPresent(tenant =>
        {
            serviceBusMessage.ApplicationProperties.Add(PropertyNames.TenantId, tenant);
        });
        return serviceBusMessage;
    }

    public static Message ToMessage(this ServiceBusReceivedMessage serviceBusMessage)
    {
        var tenantId = serviceBusMessage.ApplicationProperties.TryGetValue(PropertyNames.TenantId, out var tenant)
            ? Some(tenant as string)
            : None;
        return new(
            Id: Guid.Parse(serviceBusMessage.MessageId),
            Timestamp: Timestamp.Parse(serviceBusMessage.ApplicationProperties[PropertyNames.MessageTimestamp] as string),
            Type: serviceBusMessage.ApplicationProperties[PropertyNames.MessageType] as string,
            TenantId: tenantId,
            Content: serviceBusMessage.Body.ToString());
    }
}
