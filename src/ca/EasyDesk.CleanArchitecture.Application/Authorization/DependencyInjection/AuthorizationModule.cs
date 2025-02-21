using Autofac;
using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;

public class AuthorizationModule : AppModule
{
    private readonly Action<AuthorizationOptions>? _configure;

    public AuthorizationModule(Action<AuthorizationOptions>? configure)
    {
        _configure = configure;
    }

    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.ConfigureDispatchingPipeline(pipeline =>
        {
            pipeline
                .AddStep(typeof(HandleUnknownAgentStep<,>))
                .After(typeof(AuthenticationStep<,>));

            pipeline
                .AddStep(typeof(StaticAuthorizationStep<,>))
                .After(typeof(UnitOfWorkStep<,>))
                .After(typeof(AuthenticationStep<,>))
                .After(typeof(MultitenancyManagementStep<,>));
        });
    }

    public override void ConfigureServices(AppDescription app, IServiceCollection services, ContainerBuilder builder)
    {
        var options = new AuthorizationOptions();
        _configure?.Invoke(options);
        options.Apply(services, app);
    }
}

public static class AuthorizationModuleExtensions
{
    public static IAppBuilder AddAuthorization(this IAppBuilder builder, Action<AuthorizationOptions>? configure = null)
    {
        return builder.AddModule(new AuthorizationModule(configure));
    }

    public static bool HasAuthorization(this AppDescription app) => app.HasModule<AuthorizationModule>();
}
