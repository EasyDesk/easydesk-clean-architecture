using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.DependencyInjection;

public interface IMessageBrokerImplementation
{
    void AddCommonServices(IServiceCollection services);

    void AddMessageSender(IServiceCollection services);

    void AddMessageReceiver(IServiceCollection services);
}
