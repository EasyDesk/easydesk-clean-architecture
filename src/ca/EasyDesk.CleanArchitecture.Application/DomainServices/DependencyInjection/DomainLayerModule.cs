using Autofac;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Extensions.DependencyInjection;

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

    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        builder.RegisterType<DomainEventQueue>()
            .AsSelf()
            .As<IDomainEventNotifier>()
            .InstancePerLifetimeScope();

        builder.RegisterType<DomainEventPublisher>()
            .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes([.. app.Assemblies])
            .AssignableToOpenGenericType(typeof(IDomainEventHandler<>))
            .AsClosedTypesOf(typeof(IDomainEventHandler<>))
            .InstancePerDependency();
    }
}

public static class DomainModuleExtensions
{
    public static IAppBuilder AddDomainLayer(this IAppBuilder builder)
    {
        return builder.AddModule(new DomainLayerModule());
    }
}
