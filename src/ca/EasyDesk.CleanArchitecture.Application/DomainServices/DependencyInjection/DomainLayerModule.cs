using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.DomainServices.DependencyInjection;

public class DomainLayerModule : AppModule
{
    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.ConfigureDispatchingPipeline(pipeline =>
        {
            pipeline.AddStepAfterAll(typeof(DomainEventHandlingStep<,>));
            pipeline.AddStepAfterAll(typeof(DomainConstraintViolationsHandlingStep<,>)).Before(typeof(DomainEventHandlingStep<,>));
        });
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddScoped<DomainEventQueue>();
        services.AddScoped<IDomainEventFlusher>(provider => provider.GetRequiredService<DomainEventQueue>());
        services.AddScoped<IDomainEventNotifier>(provider => provider.GetRequiredService<DomainEventQueue>());
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        var applicationAssemblies = app.GetLayerAssemblies(CleanArchitectureLayer.Application);
        var domainAssemblies = app.GetLayerAssemblies(CleanArchitectureLayer.Domain);
        services.RegisterImplementationsAsTransient(
            typeof(IDomainEventHandler<>),
            s => s
                .FromAssemblies(applicationAssemblies)
                .FromAssemblies(domainAssemblies));
    }
}

public static class DomainModuleExtensions
{
    public static AppBuilder AddDomainLayer(this AppBuilder builder)
    {
        return builder.AddModule(new DomainLayerModule());
    }
}
