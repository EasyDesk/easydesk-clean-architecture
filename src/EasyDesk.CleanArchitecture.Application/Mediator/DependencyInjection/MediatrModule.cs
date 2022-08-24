using EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;
using EasyDesk.CleanArchitecture.Application.Modules;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Mediator.DependencyInjection;

public class MediatrModule : AppModule
{
    public MediatrModule(Action<MediatrPipelineBuilder> configurePipeline = null)
    {
        configurePipeline?.Invoke(Pipeline);
    }

    public MediatrPipelineBuilder Pipeline { get; } = new();

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddMediatR(
            app.GetLayerAssembly(CleanArchitectureLayer.Application),
            app.GetLayerAssembly(CleanArchitectureLayer.Infrastructure));

        Pipeline.AddBehavior(typeof(TransactionBehaviorWrapper<,>));
        Pipeline.AddBehavior(typeof(DomainConstraintsViolationHandlerWrapper<,>));
        Pipeline.AddBehavior(typeof(DomainEventHandlingBehaviorWrapper<,>));

        var behaviors = Pipeline.GetOrderedBehaviors().ToList();
        behaviors.ForEach(b =>
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), b);
        });
    }
}

public static class MediatrModuleExtensions
{
    public static AppBuilder AddMediatr(this AppBuilder builder, Action<MediatrPipelineBuilder> configurePipeline = null)
    {
        return builder.AddModule(new MediatrModule(configurePipeline));
    }
}
