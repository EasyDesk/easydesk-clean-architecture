using Azure.Messaging.ServiceBus;
using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus
{
    public class AzureServiceBusPublisher : IEventBusPublisher
    {
        private readonly ServiceBusSender _sender;

        public AzureServiceBusPublisher(
            ServiceBusClient client,
            AzureServiceBusSettings settings)
        {
            _sender = client.CreateSender(settings.CompleteTopicPath);
        }

        public async Task Publish(IEnumerable<EventBusMessage> messages)
        {
            var serviceBusMessages = messages.Select(m => m.ToServiceBusMessage());
            await _sender.SendMessagesAsync(serviceBusMessages);
        }

        public ValueTask DisposeAsync() => _sender.DisposeAsync();
    }
}
