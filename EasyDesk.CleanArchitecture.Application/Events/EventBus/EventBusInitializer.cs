using EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.EventBus
{
    public class EventBusInitializer : IHostedService
    {
        private readonly IEventBusSetup _eventBusSetup;
        private readonly IServiceProvider _serviceProvider;

        public EventBusInitializer(
            IEventBusSetup eventBusSetup,
            IServiceProvider serviceProvider)
        {
            _eventBusSetup = eventBusSetup;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _eventBusSetup.SetupDefaults();

            await SetupPublisher();
            await SetupConsumer();
        }

        private async Task SetupPublisher()
        {
            var publisher = _serviceProvider.GetService<IEventBusPublisher>();
            if (publisher is null)
            {
                return;
            }
            await _eventBusSetup.SetupPublisher();
        }

        private async Task SetupConsumer()
        {
            var consumer = _serviceProvider.GetService<IEventBusConsumer>();
            if (consumer is null)
            {
                return;
            }

            var consumerDefinition = _serviceProvider.GetRequiredService<EventBusConsumerDefinition>();
            await _eventBusSetup.SetupConsumer(consumerDefinition);
            await consumer.StartListening();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
