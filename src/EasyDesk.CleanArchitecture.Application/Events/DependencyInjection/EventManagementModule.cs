using EasyDesk.CleanArchitecture.Application.Modules;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;

public class EventManagementModule : IAppModule
{
    private readonly Action<EventManagementOptions> _configure;

    public EventManagementModule(IEventBusImplementation implementation, Action<EventManagementOptions> configure)
    {
        Implementation = implementation;
        _configure = configure;
    }

    public IEventBusImplementation Implementation { get; }

    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        Implementation.AddCommonServices(services);
        var options = new EventManagementOptions(services, Implementation, app);
        _configure(options);
    }
}

public static class EventManagementModuleExtensions
{
    public static AppBuilder AddEventManagement(this AppBuilder builder, IEventBusImplementation implementation, Action<EventManagementOptions> configure)
    {
        return builder.AddModule(new EventManagementModule(implementation, configure));
    }
}
