using EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Features;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Startup.Features;

public class EventManagementFeature : IAppFeature
{
    public EventManagementFeature(IEventBusImplementation implementation, bool usesPublisher, bool usesConsumer)
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
            var dataAccessImplementation = app.RequireFeature<DataAccessFeature>().Implementation;
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

public static class EventManagementFeatureExtensions
{
    public static AppBuilder AddEventManagement(this AppBuilder builder, IEventBusImplementation implementation, bool usesPublisher, bool usesConsumer)
    {
        return builder.AddFeature(new EventManagementFeature(implementation, usesPublisher, usesConsumer));
    }
}
