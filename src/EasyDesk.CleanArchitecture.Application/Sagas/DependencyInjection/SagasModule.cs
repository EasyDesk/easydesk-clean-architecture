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
            .SubtypesOrImplementationsOf(typeof(ISagaController<,,>))
            .NonAbstract()
            .FindTypes()
            .SelectMany(c => c.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISagaController<,,>))
            .Select(i => GetType()
                .GetMethod(nameof(ConfigureSaga), BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(i.GetGenericArguments()))
            .ForEach(m => m.Invoke(this, configureSagaArgs));
    }

    private void ConfigureSaga<TController, TId, TState>(IServiceCollection services)
        where TController : class, ISagaController<TController, TId, TState>
    {
        var sagaBuilder = new SagaBuilder<TController, TId, TState>(services);
        TController.ConfigureSaga(sagaBuilder);
        services.AddTransient<TController>();
    }

    private class SagaBuilder<TController, TId, TState> : ISagaBuilder<TController, TId, TState>
        where TController : class, ISagaController<TController, TId, TState>
    {
        private readonly IServiceCollection _services;

        public SagaBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public ISagaBuilder<TController, TId, TState> For<T, R>(SagaRequestConfiguration<T, R, TController, TId, TState> configuration) where T : IDispatchable<R>
        {
            _services.AddSingleton(configuration);
            _services.AddTransient<IHandler<T, R>, SagaHandler<T, R, TController, TId, TState>>();
            return this;
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
