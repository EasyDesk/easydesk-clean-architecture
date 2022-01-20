using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;

public class EventManagementModule : IAppModule
{
    public EventManagementModule(IEventBusImplementation implementation, bool usesPublisher, bool usesConsumer)
    {
        Implementation = implementation;
        UsesPublisher = usesPublisher;
        UsesConsumer = usesConsumer;
    }

    public bool UsesPublisher { get; }

    public bool UsesConsumer { get; }

    public IEventBusImplementation Implementation { get; }

    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        if (UsesConsumer || UsesPublisher)
        {
            var builder = services.AddEventManagement(Implementation, app);
            var dataAccessImplementation = app.RequireModule<DataAccessModule>().Implementation;
            if (UsesPublisher)
            {
                builder.AddOutboxPublisher();
                dataAccessImplementation.AddOutbox(services, app);
            }
            if (UsesConsumer)
            {
                builder.AddIdempotentConsumer(app.ApplicationAssemblyMarker);
                dataAccessImplementation.AddIdempotenceManager(services, app);
            }
        }
    }
}

public static class EventManagementModuleExtensions
{
    public static AppBuilder AddEventManagement(this AppBuilder builder, IEventBusImplementation implementation, bool usesPublisher, bool usesConsumer)
    {
        return builder.AddModule(new EventManagementModule(implementation, usesPublisher, usesConsumer));
    }
}
