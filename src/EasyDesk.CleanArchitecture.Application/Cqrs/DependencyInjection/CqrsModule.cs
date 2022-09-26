using EasyDesk.CleanArchitecture.Application.Cqrs.Handlers;
using EasyDesk.CleanArchitecture.Application.Cqrs.Pipeline;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Cqrs.DependencyInjection;

public class CqrsModule : AppModule
{
    public CqrsModule(Action<PipelineBuilder> configurePipeline = null)
    {
        configurePipeline?.Invoke(Pipeline);
    }

    public PipelineBuilder Pipeline { get; } = new();

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddTransient<ICqrsRequestDispatcher, CqrsRequestDispatcher>();

        RegisterRequestHandlers(services, app);

        Pipeline.AddStep(typeof(UnitOfWorkStep<,>));
        Pipeline.AddStep(typeof(DomainEventHandlingStep<,>));

        var steps = Pipeline.GetOrderedSteps().ToList();
        steps.ForEach(b =>
        {
            services.AddTransient(typeof(IPipelineStep<,>), b);
        });
    }

    private void RegisterRequestHandlers(IServiceCollection services, AppDescription app)
    {
        services.RegisterImplementationsAsTransient(
            typeof(ICqrsRequestHandler<,>),
            s => s.FromAssemblies(
                app.GetLayerAssembly(CleanArchitectureLayer.Application),
                app.GetLayerAssembly(CleanArchitectureLayer.Infrastructure)));
    }
}

public static class MediatrModuleExtensions
{
    public static AppBuilder AddMediatr(this AppBuilder builder, Action<PipelineBuilder> configurePipeline = null)
    {
        return builder.AddModule(new CqrsModule(configurePipeline));
    }
}
