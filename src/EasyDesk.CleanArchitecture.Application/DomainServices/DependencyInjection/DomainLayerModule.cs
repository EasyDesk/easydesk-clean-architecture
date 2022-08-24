using System.Reflection;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Reflection;
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
        RegisterImplementationsForOpenGenericInterface(services, typeof(IDomainEventHandler<>), applicationAssembly, domainAssembly);
        RegisterImplementationsForOpenGenericInterface(services, typeof(IDomainEventPropagator<>), applicationAssembly);
    }

    private void RegisterImplementationsForOpenGenericInterface(IServiceCollection services, Type interfaceType, params Assembly[] assembliesToScan)
    {
        new AssemblyScanner()
            .FromAssemblies(assembliesToScan)
            .NonAbstract()
            .SubtypesOrImplementationsOf(interfaceType)
            .FindTypes()
            .ForEach(t => RegisterAsClosedGenericInterfaces(services, t, interfaceType));
    }

    private void RegisterAsClosedGenericInterfaces(IServiceCollection services, Type implementationType, Type openInterfaceType)
    {
        implementationType
            .GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openInterfaceType)
            .ForEach(i => services.AddTransient(i, implementationType));
    }
}

public static class DomainModuleExtensions
{
    public static AppBuilder AddDomainLayer(this AppBuilder builder)
    {
        return builder.AddModule(new DomainLayerModule());
    }
}
