using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;

public class DispatchingModule : AppModule
{
    public DispatchingModule(Action<PipelineBuilder>? configurePipeline = null)
    {
        configurePipeline?.Invoke(Pipeline);
    }

    public PipelineBuilder Pipeline { get; } = new();

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddTransient<IDispatcher, Dispatcher>();

        RegisterRequestHandlers(services, app);

        // TODO: Move away these steps that do not belong here
        Pipeline.AddStep(typeof(DomainEventHandlingStep<,>));
        Pipeline.AddStep(typeof(DomainConstraintViolationsHandlingStep<,>)).Before(typeof(DomainEventHandlingStep<,>));
        var steps = Pipeline.GetOrderedSteps().ToList();
        services.AddHostedService<StartupPipelineLogger>(sp => new(steps, sp.GetRequiredService<ILogger<StartupPipelineLogger>>()));
        services.AddSingleton<IPipeline>(p => new GenericPipeline(steps));
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

public static class DispatchingModuleExtensions
{
    public static AppBuilder AddDispatching(this AppBuilder builder, Action<PipelineBuilder>? configurePipeline = null)
    {
        return builder.AddModule(new DispatchingModule(configurePipeline));
    }

    public static AppDescription ConfigureDispatchingPipeline(this AppDescription app, Action<PipelineBuilder> configure)
    {
        configure(app.RequireModule<DispatchingModule>().Pipeline);
        return app;
    }
}
