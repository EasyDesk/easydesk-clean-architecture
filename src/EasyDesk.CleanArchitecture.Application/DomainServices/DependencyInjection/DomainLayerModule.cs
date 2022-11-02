using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.DomainServices.DependencyInjection;

public class DomainLayerModule : AppModule
{
    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddScoped<DomainEventQueue>();
        services.AddScoped<IDomainEventFlusher>(provider => provider.GetRequiredService<DomainEventQueue>());
        services.AddScoped<IDomainEventNotifier>(provider => provider.GetRequiredService<DomainEventQueue>());
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        var applicationAssembly = app.GetLayerAssembly(CleanArchitectureLayer.Application);
        var domainAssembly = app.GetLayerAssembly(CleanArchitectureLayer.Domain);
        services.RegisterImplementationsAsTransient(
            typeof(IDomainEventHandler<>),
            s => s.FromAssemblies(applicationAssembly, domainAssembly));
    }
}

public static class DomainModuleExtensions
{
    public static AppBuilder AddDomainLayer(this AppBuilder builder)
    {
        return builder.AddModule(new DomainLayerModule());
    }
}
