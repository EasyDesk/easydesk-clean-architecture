﻿using Azure.Messaging.ServiceBus;
using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using EasyDesk.CleanArchitecture.Messaging.ServiceBus.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Messaging.ServiceBus.Publisher;

public class AzureServiceBusPublisher : IEventBusPublisher
{
    private readonly ServiceBusSender _sender;

    public AzureServiceBusPublisher(
        ServiceBusClient client,
        AzureServiceBusSenderDescriptor descriptor)
    {
        _sender = descriptor.Match(
            queue: q => client.CreateSender(q),
            topic: t => client.CreateSender(t));
    }

    public async Task Publish(IEnumerable<EventBusMessage> messages)
    {
        var serviceBusMessages = messages.Select(m => m.ToServiceBusMessage());
        await _sender.SendMessagesAsync(serviceBusMessages);
    }

    public ValueTask DisposeAsync() => _sender.DisposeAsync();
}