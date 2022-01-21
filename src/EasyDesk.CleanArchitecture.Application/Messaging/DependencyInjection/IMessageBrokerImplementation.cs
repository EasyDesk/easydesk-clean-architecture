using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Messaging.DependencyInjection;

public interface IMessageBrokerImplementation
{
    void AddCommonServices(IServiceCollection services);

    void AddMessagePublisher(IServiceCollection services);

    void AddMessageConsumer(IServiceCollection services);
}
