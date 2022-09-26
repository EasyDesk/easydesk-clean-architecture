using EasyDesk.CleanArchitecture.Application.Cqrs.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;

public class AuthorizationModule : AppModule
{
    private readonly Action<AuthorizationOptions> _configure;

    public AuthorizationModule(Action<AuthorizationOptions> configure)
    {
        _configure = configure;
    }

    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.RequireModule<CqrsModule>().Pipeline.AddStep(typeof(AuthorizationStep<,>));
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        var options = new AuthorizationOptions(services, app);
        services.AddScoped(typeof(IAuthorizer<>), typeof(NoAuthorizer<>));
        _configure?.Invoke(options);
    }
}

public static class AuthorizationModuleExtensions
{
    public static AppBuilder AddAuthorization(this AppBuilder builder, Action<AuthorizationOptions> configure = null)
    {
        return builder.AddModule(new AuthorizationModule(configure));
    }

    public static bool HasAuthorization(this AppDescription app) => app.HasModule<AuthorizationModule>();
}
