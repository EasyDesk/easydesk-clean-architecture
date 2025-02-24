using Autofac;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;

namespace EasyDesk.CleanArchitecture.Application.Logging.DependencyInjection;

public class LoggingModule : AppModule
{
    private readonly LoggingConfiguration _configuration;

    public LoggingModule(LoggingConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.ConfigureDispatchingPipeline(pipeline => pipeline
            .AddStepBeforeAll(typeof(LoggingStep<,>)));
    }

    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        builder.RegisterInstance(_configuration)
            .SingleInstance();
    }
}

public static class LoggingModuleExtensions
{
    public static IAppBuilder AddLogging(this IAppBuilder builder, Action<LoggingConfiguration>? configure = null)
    {
        var configuration = new LoggingConfiguration();
        configure?.Invoke(configuration);
        return builder.AddModule(new LoggingModule(configuration));
    }
}
