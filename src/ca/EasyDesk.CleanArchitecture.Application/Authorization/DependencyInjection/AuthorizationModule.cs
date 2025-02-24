using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;

namespace EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;

public class AuthorizationModule : AppModule
{
    private readonly AuthorizationOptions _options;

    public AuthorizationModule(AuthorizationOptions options)
    {
        _options = options;
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

    protected override void ConfigureRegistry(AppDescription app, ServiceRegistry registry)
    {
        _options.Apply(registry, app);
    }
}

public static class AuthorizationModuleExtensions
{
    public static IAppBuilder AddAuthorization(this IAppBuilder builder, Action<AuthorizationOptions>? configure = null)
    {
        var options = new AuthorizationOptions();
        configure?.Invoke(options);
        return builder.AddModule(new AuthorizationModule(options));
    }

    public static bool HasAuthorization(this AppDescription app) => app.HasModule<AuthorizationModule>();
}
