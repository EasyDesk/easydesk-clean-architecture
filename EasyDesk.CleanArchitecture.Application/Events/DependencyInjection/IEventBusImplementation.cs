using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Events.DependencyInjection
{
    public interface IEventBusImplementation
    {
        void AddCommonServices(IServiceCollection services);

        void AddPublisher(IServiceCollection services);

        void AddConsumer(IServiceCollection services);
    }
}
