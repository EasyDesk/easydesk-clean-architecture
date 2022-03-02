using System.Linq;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.DomainServices.DependencyInjection;

public class DomainServicesModule : IAppModule
{
    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddScoped<DomainEventQueue>();
        services.AddScoped<IDomainEventNotifier>(provider => provider.GetRequiredService<DomainEventQueue>());
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        services.AddTransient(typeof(IDomainEventHandler<>), typeof(PropagateDomainEvent<>));

        var handlerTypes = ReflectionUtils.InstantiableSubtypesOfGenericInterface(
            typeof(IDomainEventHandler<>), app.ApplicationAssemblyMarker);
        var propagatorTypes = ReflectionUtils.InstantiableSubtypesOfGenericInterface(
            typeof(IDomainEventPropagator<>), app.ApplicationAssemblyMarker);

        foreach (var (interfaceType, implementation) in handlerTypes.Concat(propagatorTypes))
        {
            services.AddTransient(interfaceType, implementation);
        }
    }
}

public static class DomainModuleExtensions
{
    public static AppBuilder AddDomain(this AppBuilder builder)
    {
        return builder.AddModule(new DomainServicesModule());
    }
}
