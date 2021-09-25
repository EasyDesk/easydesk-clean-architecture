using EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus
{
    public class AzureServiceBusSetup : IEventBusSetup
    {
        private readonly AzureServiceBusAdministrationFacade _administrationFacade;
        private readonly AzureServiceBusSettings _settings;

        public AzureServiceBusSetup(
            AzureServiceBusAdministrationFacade administrationFacade,
            AzureServiceBusSettings settings)
        {
            _administrationFacade = administrationFacade;
            _settings = settings;
        }

        public async Task SetupDefaults() => await _administrationFacade.CreateTopicIdempotent(_settings.CompleteTopicPath);

        public Task SetupPublisher() => Task.CompletedTask;

        public async Task SetupConsumer(EventBusConsumerDefinition consumerDefinition)
        {
            var filter = new EventTypeFilter(consumerDefinition.EventTypes);
            await _administrationFacade.CreateSubscriptionIdempotent(
                _settings.CompleteTopicPath,
                _settings.SubscriptionName,
                filter);
        }
    }
}
