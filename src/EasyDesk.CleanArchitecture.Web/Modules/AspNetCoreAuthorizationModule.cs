using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Modules;

public class AspNetCoreAuthorizationModule : AppModule
{
    private readonly Action<AuthorizationOptions> _configure;

    public AspNetCoreAuthorizationModule(Action<AuthorizationOptions> configure = null)
    {
        _configure = configure;
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddAuthorization(options => _configure?.Invoke(options));
    }
}

public static class AspNetCoreAuthorizationModuleExtensions
{
    public static AppBuilder AddAspNetCoreAuthorization(this AppBuilder builder, Action<AuthorizationOptions> configure = null)
    {
        return builder.AddModule(new AspNetCoreAuthorizationModule(configure));
    }

    public static bool HasAspNetCoreAuthorization(this AppDescription app) => app.HasModule<AspNetCoreAuthorizationModule>();
}
