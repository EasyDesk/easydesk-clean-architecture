using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;

public class DispatchingModule : AppModule
{
    public DispatchingModule(Action<PipelineBuilder> configurePipeline = null)
    {
        configurePipeline?.Invoke(Pipeline);
    }

    public PipelineBuilder Pipeline { get; } = new();

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddTransient<IDispatcher, Dispatcher>();

        RegisterRequestHandlers(services, app);

        Pipeline.AddStep(typeof(UnitOfWorkStep<,>));
        Pipeline.AddStep(typeof(DomainEventHandlingStep<,>));

        var steps = Pipeline.GetOrderedSteps().ToList();
        services.AddScoped<IPipeline>(p => new GenericPipeline(p, steps));
    }

    private void RegisterRequestHandlers(IServiceCollection services, AppDescription app)
    {
        services.RegisterImplementationsAsTransient(
            typeof(IHandler<,>),
            s => s.FromAssemblies(
                app.GetLayerAssembly(CleanArchitectureLayer.Application),
                app.GetLayerAssembly(CleanArchitectureLayer.Infrastructure)));
    }
}

public static class MediatrModuleExtensions
{
    public static AppBuilder AddMediatr(this AppBuilder builder, Action<PipelineBuilder> configurePipeline = null)
    {
        return builder.AddModule(new DispatchingModule(configurePipeline));
    }
}
