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
        services.AddScoped<DomainEventScope>();
        services.AddScoped<IDomainEventNotifier>(provider => provider.GetRequiredService<DomainEventScope>());
        services.AddScoped<DomainEventPublisher>();

        services.RegisterImplementationsAsTransient(
            typeof(IDomainEventHandler<>),
            s => s.FromAssemblies(app.Assemblies));
    }
}

public static class DomainModuleExtensions
{
    public static IAppBuilder AddDomainLayer(this IAppBuilder builder)
    {
        return builder.AddModule(new DomainLayerModule());
    }
}
