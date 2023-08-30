using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Extensions.DependencyInjection;
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

        services.RegisterImplementationsAsTransient(
            typeof(IDomainEventHandler<>),
            s => s
                .FromAssemblies(app.GetLayerAssemblies(CleanArchitectureLayer.Application))
                .FromAssemblies(app.GetLayerAssemblies(CleanArchitectureLayer.Domain))
                .FromAssemblies(app.GetLayerAssemblies(CleanArchitectureLayer.Infrastructure)));
    }
}

public static class DomainModuleExtensions
{
    public static AppBuilder AddDomainLayer(this AppBuilder builder)
    {
        return builder.AddModule(new DomainLayerModule());
    }
}
