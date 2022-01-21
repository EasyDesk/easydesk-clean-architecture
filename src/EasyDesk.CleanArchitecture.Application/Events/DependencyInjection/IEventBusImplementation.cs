using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;

public interface IEventBusImplementation
{
    void AddCommonServices(IServiceCollection services);

    void AddEventBusPublisher(IServiceCollection services);

    void AddEventBusConsumer(IServiceCollection services);
}
