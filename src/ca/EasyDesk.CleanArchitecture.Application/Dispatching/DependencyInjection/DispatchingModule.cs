using Autofac;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;

public class DispatchingModule : AppModule
{
    public DispatchingModule(Action<PipelineBuilder>? configurePipeline = null)
    {
        configurePipeline?.Invoke(Pipeline);
    }

    public PipelineBuilder Pipeline { get; } = new();

    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        builder.RegisterType<Dispatcher>().As<IDispatcher>().InstancePerDependency();

        RegisterRequestHandlers(builder, app);

        var steps = Pipeline.GetOrderedSteps().ToList();
        builder.Register(c => new StartupPipelineLogger(steps, c.Resolve<ILogger<StartupPipelineLogger>>(), c.Resolve<ILifetimeScope>()))
            .As<IHostedService>()
            .SingleInstance();

        builder.RegisterInstance(new GenericPipelineProvider(steps))
            .As<IPipelineProvider>()
            .SingleInstance();

        builder.RegisterType<DispatcherFactory>()
            .InstancePerLifetimeScope();
    }

    private void RegisterRequestHandlers(ContainerBuilder builder, AppDescription app)
    {
        builder
            .RegisterAssemblyTypes([.. app.Assemblies,])
            .AssignableToOpenGenericType(typeof(IHandler<,>))
            .AsClosedTypesOf(typeof(IHandler<,>))
            .InstancePerDependency();
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
