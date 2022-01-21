using Azure.Messaging.ServiceBus;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Messaging.Sender;
using EasyDesk.CleanArchitecture.Messaging.ServiceBus.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Messaging.ServiceBus.Sender;

public sealed class AzureServiceBusSender : IMessageSender
{
    private readonly ServiceBusSender _sender;

    public AzureServiceBusSender(
        ServiceBusClient client,
        AzureServiceBusSenderDescriptor descriptor)
    {
        _sender = descriptor.Match(
            queue: q => client.CreateSender(q),
            topic: t => client.CreateSender(t));
    }

    public async Task Send(IEnumerable<Message> messages)
    {
        var serviceBusMessages = messages.Select(m => m.ToServiceBusMessage());
        await _sender.SendMessagesAsync(serviceBusMessages);
    }

    public async void Dispose() => await _sender.DisposeAsync();
}
