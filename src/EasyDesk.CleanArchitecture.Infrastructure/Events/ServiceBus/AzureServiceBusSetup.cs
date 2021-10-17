using EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus
{
    public class AzureServiceBusSetup : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AzureServiceBusAdministrationFacade _administrationFacade;

        public AzureServiceBusSetup(
            IServiceProvider serviceProvider,
            AzureServiceBusAdministrationFacade administrationFacade)
        {
            _serviceProvider = serviceProvider;
            _administrationFacade = administrationFacade;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var sender = _serviceProvider.GetService<AzureServiceBusSenderDescriptor>();
            if (sender is not null)
            {
                await sender.Match(
                    queue: q => _administrationFacade.CreateQueueIdempotent(q),
                    topic: t => _administrationFacade.CreateTopicIdempotent(t));
            }

            var receiver = _serviceProvider.GetService<AzureServiceBusReceiverDescriptor>();
            if (receiver is not null)
            {
                await receiver.Match(
                    queue: q => _administrationFacade.CreateQueueIdempotent(q),
                    subscription: async (t, s) =>
                    {
                        var consumerDefinition = _serviceProvider.GetRequiredService<EventBusConsumerDefinition>();
                        var filter = new EventTypeFilter(consumerDefinition.EventTypes);
                        await _administrationFacade.CreateTopicIdempotent(t);
                        await _administrationFacade.CreateSubscriptionIdempotent(t, s, filter);
                    });

                var consumer = _serviceProvider.GetRequiredService<IEventBusConsumer>();
                await consumer.StartListening();
            }
        }
    }
}
