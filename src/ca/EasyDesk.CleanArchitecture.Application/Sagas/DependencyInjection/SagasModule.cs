using Autofac;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.CleanArchitecture.Application.Sagas.Builder;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Reflection;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Sagas.DependencyInjection;

public class SagasModule : AppModule
{
    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.ConfigureDispatchingPipeline(pipeline =>
        {
            pipeline
                .AddStepAfterAll(typeof(SaveSagaChangesStep<,>))
                .Before(typeof(SaveChangesStep<,>))
                .Before(typeof(DomainEventHandlingStep<,>));
        });
    }

    protected override void ConfigureRegistry(AppDescription app, ServiceRegistry registry)
    {
        app.RequireModule<DataAccessModule>().Implementation.AddSagas(registry, app);
    }

    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        var configureSagaArgs = new[] { builder };
        new AssemblyScanner()
            .FromAssemblies(app.Assemblies)
            .SubtypesOrImplementationsOf(typeof(ISagaController<,>))
            .NonAbstract()
            .FindTypes()
            .SelectMany(c => c.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISagaController<,>))
                .Select(i => GetType()
                    .GetMethod(nameof(ConfigureSaga), BindingFlags.NonPublic | BindingFlags.Instance)!
                    .MakeGenericMethod([.. i.GetGenericArguments(), c])))
            .ForEach(m => m.Invoke(this, configureSagaArgs));

        builder.RegisterGeneric(typeof(SagaCoordinator<,>))
            .As(typeof(ISagaCoordinator<,>))
            .InstancePerLifetimeScope();

        builder.RegisterType<SagaRegistry>()
            .InstancePerLifetimeScope();
    }

    private void ConfigureSaga<TId, TState, TController>(ContainerBuilder builder)
        where TController : class, ISagaController<TId, TState>
    {
        var sink = new SagaConfigurationSink<TId, TState>(builder);
        var sagaBuilder = new SagaBuilder<TId, TState>(sink);
        TController.ConfigureSaga(sagaBuilder);
        builder.RegisterType<TController>()
            .InstancePerDependency();
    }

    private class SagaConfigurationSink<TId, TState> : ISagaConfigurationSink<TId, TState>
    {
        private readonly ContainerBuilder _builder;

        public SagaConfigurationSink(ContainerBuilder builder)
        {
            _builder = builder;
        }

        public void RegisterRequestConfiguration<T, R>(SagaStepConfiguration<T, R, TId, TState> configuration)
            where T : IDispatchable<R>
        {
            _builder.RegisterInstance(configuration)
                .SingleInstance();

            _builder.RegisterType<SagaRequestHandler<T, R, TId, TState>>()
                .As<IHandler<T, R>>()
                .InstancePerDependency();
        }

        public void RegisterEventConfiguration<T>(SagaStepConfiguration<T, Nothing, TId, TState> configuration)
            where T : DomainEvent
        {
            _builder.RegisterInstance(configuration)
                .SingleInstance();

            _builder.RegisterType<SagaEventHandler<T, TId, TState>>()
                .As<IDomainEventHandler<T>>()
                .InstancePerDependency();
        }
    }
}

public static class SagasModuleExtensions
{
    public static IAppBuilder AddSagas(this IAppBuilder builder)
    {
        return builder.AddModule(new SagasModule());
    }
}
