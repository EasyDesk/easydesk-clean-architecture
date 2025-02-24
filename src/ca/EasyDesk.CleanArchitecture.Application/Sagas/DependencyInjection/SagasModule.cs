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
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Sagas.DependencyInjection;

public class SagasModule : AppModule
{
    public override void ConfigureServices(AppDescription app, IServiceCollection services, ContainerBuilder builder)
    {
        var configureSagaArgs = new[] { services };
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

        app.RequireModule<DataAccessModule>().Implementation.AddSagas(services, app);

        services.AddScoped(typeof(ISagaCoordinator<,>), typeof(SagaCoordinator<,>));
        services.AddScoped<SagaRegistry>();
        app.ConfigureDispatchingPipeline(pipeline =>
        {
            pipeline
                .AddStepAfterAll(typeof(SaveSagaChangesStep<,>))
                .Before(typeof(SaveChangesStep<,>))
                .Before(typeof(DomainEventHandlingStep<,>));
        });
    }

    private void ConfigureSaga<TId, TState, TController>(IServiceCollection services)
        where TController : class, ISagaController<TId, TState>
    {
        var sink = new SagaConfigurationSink<TId, TState>(services);
        var sagaBuilder = new SagaBuilder<TId, TState>(sink);
        TController.ConfigureSaga(sagaBuilder);
        services.AddTransient<TController>();
    }

    private class SagaConfigurationSink<TId, TState> : ISagaConfigurationSink<TId, TState>
    {
        private readonly IServiceCollection _services;

        public SagaConfigurationSink(IServiceCollection services)
        {
            _services = services;
        }

        public void RegisterRequestConfiguration<T, R>(SagaStepConfiguration<T, R, TId, TState> configuration)
            where T : IDispatchable<R>
        {
            _services.AddSingleton(configuration);
            _services.AddTransient<IHandler<T, R>, SagaRequestHandler<T, R, TId, TState>>();
        }

        public void RegisterEventConfiguration<T>(SagaStepConfiguration<T, Nothing, TId, TState> configuration)
            where T : DomainEvent
        {
            _services.AddSingleton(configuration);
            _services.AddTransient<IDomainEventHandler<T>, SagaEventHandler<T, TId, TState>>();
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
