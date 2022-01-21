using EasyDesk.CleanArchitecture.Application.Modules;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyDesk.CleanArchitecture.Application.Messaging.DependencyInjection;

public class MessagingModule : IAppModule
{
    private readonly Action<MessagingOptions> _configure;

    public MessagingModule(IMessageBrokerImplementation implementation, Action<MessagingOptions> configure)
    {
        Implementation = implementation;
        _configure = configure;
    }

    public IMessageBrokerImplementation Implementation { get; }

    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        Implementation.AddCommonServices(services);
        var options = new MessagingOptions(services, Implementation, app);
        _configure(options);
    }
}

public static class MessagingModuleExtensions
{
    public static AppBuilder AddMessaging(this AppBuilder builder, IMessageBrokerImplementation implementation, Action<MessagingOptions> configure)
    {
        return builder.AddModule(new MessagingModule(implementation, configure));
    }
}
