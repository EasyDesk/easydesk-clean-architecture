using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.DomainServices.DependencyInjection;

public class DomainLayerModule : AppModule
{
    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddScoped<DomainEventQueue>();
        services.AddScoped<IDomainEventNotifier>(provider => provider.GetRequiredService<DomainEventQueue>());
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        services.AddTransient(typeof(IDomainEventHandler<>), typeof(PropagateDomainEvent<>));

        var applicationAssembly = app.GetLayerAssembly(CleanArchitectureLayer.Application);
        var domainAssembly = app.GetLayerAssembly(CleanArchitectureLayer.Domain);
        services.RegisterImplementationsAsTransient(
            typeof(IDomainEventHandler<>),
            s => s.FromAssemblies(applicationAssembly, domainAssembly));
        services.RegisterImplementationsAsTransient(
            typeof(IDomainEventPropagator<>),
            s => s.FromAssemblies(applicationAssembly));
    }
}

public static class DomainModuleExtensions
{
    public static AppBuilder AddDomainLayer(this AppBuilder builder)
    {
        return builder.AddModule(new DomainLayerModule());
    }
}
