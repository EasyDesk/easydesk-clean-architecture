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
        _configure(options);
    }
}

public static class AuthorizationModuleExtensions
{
    public static AppBuilder AddAuthorization(this AppBuilder builder, Action<AuthorizationOptions> configure)
    {
        return builder.AddModule(new AuthorizationModule(configure));
    }

    public static bool HasAuthorization(this AppDescription app) => app.HasModule<AuthorizationModule>();
}
