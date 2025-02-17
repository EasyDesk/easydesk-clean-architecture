using Autofac;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Logging.DependencyInjection;

public class LoggingModule : AppModule
{
    private readonly Action<LoggingConfiguration>? _configure;

    public LoggingModule(Action<LoggingConfiguration>? configure = null)
    {
        _configure = configure;
    }

    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.ConfigureDispatchingPipeline(pipeline => pipeline
            .AddStepBeforeAll(typeof(LoggingStep<,>)));
    }

    public override void ConfigureServices(AppDescription app, IServiceCollection services, ContainerBuilder builder)
    {
        var config = new LoggingConfiguration();
        _configure?.Invoke(config);

        services.AddSingleton(config);
    }
}

public static class LoggingModuleExtensions
{
    public static IAppBuilder AddLogging(this IAppBuilder builder, Action<LoggingConfiguration>? configure = null)
    {
        return builder.AddModule(new LoggingModule(configure));
    }
}
