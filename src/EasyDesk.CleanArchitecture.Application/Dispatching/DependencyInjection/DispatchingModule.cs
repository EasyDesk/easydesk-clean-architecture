using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.Tools.Collections;
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

        Pipeline.AddStep(typeof(DomainEventHandlingStep<,>));

        var steps = Pipeline.GetOrderedSteps().ToList();
        var pipelineDesc = "------------------------------------------------------------------------------\nRequest pipeline\n------------------------------------------------------------------------------\n";
        var stepsEnumerable = steps.Select((s, i) => $"{i + 1}. {s.FullName}");
        pipelineDesc += stepsEnumerable.Append("-------- Handler --------").Concat(stepsEnumerable.Reverse()).ConcatStrings("\n", "\n", "\n");
        pipelineDesc += "\n------------------------------------------------------------------------------";
        Console.WriteLine(pipelineDesc);
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

public static class DispatchingModuleExtensions
{
    public static AppBuilder AddDispatching(this AppBuilder builder, Action<PipelineBuilder> configurePipeline = null)
    {
        return builder.AddModule(new DispatchingModule(configurePipeline));
    }

    public static AppDescription ConfigureDispatchingPipeline(this AppDescription app, Action<PipelineBuilder> configure)
    {
        configure(app.RequireModule<DispatchingModule>().Pipeline);
        return app;
    }
}
