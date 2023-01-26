using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Sagas.DependencyInjection;

public class SagasModule : AppModule
{
    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        var configureSagaArgs = new[] { services };
        new AssemblyScanner()
            .FromAssemblies(app.GetLayerAssembly(CleanArchitectureLayer.Application))
            .SubtypesOrImplementationsOf(typeof(ISagaController<,>))
            .NonAbstract()
            .FindTypes()
            .SelectMany(c => c.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISagaController<,>))
                .Select(i => GetType()
                    .GetMethod(nameof(ConfigureSaga), BindingFlags.NonPublic | BindingFlags.Instance)!
                    .MakeGenericMethod(i.GetGenericArguments().Append(c).ToArray())))
            .ForEach(m => m.Invoke(this, configureSagaArgs));

        app.RequireModule<DataAccessModule>().Implementation.AddSagas(services, app);
    }

    private void ConfigureSaga<TId, TState, TController>(IServiceCollection services)
        where TId : notnull
        where TState : notnull
        where TController : class, ISagaController<TId, TState>
    {
        var sink = new SagaConfigurationSink<TId, TState>(services);
        var sagaBuilder = new SagaBuilder<TId, TState>(sink);
        TController.ConfigureSaga(sagaBuilder);
        services.AddTransient<TController>();
    }

    private class SagaConfigurationSink<TId, TState> : ISagaConfigurationSink<TId, TState>
        where TId : notnull
    {
        private readonly IServiceCollection _services;

        public SagaConfigurationSink(IServiceCollection services)
        {
            _services = services;
        }

        public void RegisterConfiguration<T, R>(SagaRequestConfiguration<T, R, TId, TState> configuration) where T : IDispatchable<R> where R : notnull
        {
            _services.AddSingleton(configuration);
            _services.AddTransient<IHandler<T, R>, SagaHandler<T, R, TId, TState>>();
        }
    }
}

public static class SagasModuleExtensions
{
    public static AppBuilder AddSagas(this AppBuilder builder)
    {
        return builder.AddModule(new SagasModule());
    }
}
