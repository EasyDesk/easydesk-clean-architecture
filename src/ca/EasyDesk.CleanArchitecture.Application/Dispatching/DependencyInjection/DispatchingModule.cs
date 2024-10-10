using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.Extensions.DependencyInjection;
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

        var steps = Pipeline.GetOrderedSteps().ToList();
        services.AddHostedService<StartupPipelineLogger>(sp => new(steps, sp.GetRequiredService<ILogger<StartupPipelineLogger>>()));
        services.AddSingleton<IPipelineProvider>(p => new GenericPipelineProvider(steps));
        services.AddTransient<IPipeline, DefaultPipeline>();
    }

    private void RegisterRequestHandlers(IServiceCollection services, AppDescription app)
    {
        services.RegisterImplementationsAsTransient(
            typeof(IHandler<,>),
            s => s.FromAssemblies(app.Assemblies));
    }
}

public static class DispatchingModuleExtensions
{
    public static IAppBuilder AddDispatching(this IAppBuilder builder, Action<PipelineBuilder>? configurePipeline = null)
    {
        return builder.AddModule(new DispatchingModule(configurePipeline));
    }

    public static AppDescription ConfigureDispatchingPipeline(this AppDescription app, Action<PipelineBuilder> configure)
    {
        configure(app.RequireModule<DispatchingModule>().Pipeline);
        return app;
    }
}
