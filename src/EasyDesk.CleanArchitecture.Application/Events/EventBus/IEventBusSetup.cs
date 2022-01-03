using EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.EventBus;

public interface IEventBusSetup
{
    Task SetupDefaults();

    Task SetupPublisher();

    Task SetupConsumer(EventBusConsumerDefinition consumerDefinition);
}
