using EasyDesk.CleanArchitecture.Application.Modules;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;

public class AuthorizationModule : IAppModule
{
    private readonly Action<AuthorizationOptions> _configure;

    public AuthorizationModule(Action<AuthorizationOptions> configure)
    {
        _configure = configure;
    }

    public void ConfigureServices(IServiceCollection services, AppDescription app)
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
