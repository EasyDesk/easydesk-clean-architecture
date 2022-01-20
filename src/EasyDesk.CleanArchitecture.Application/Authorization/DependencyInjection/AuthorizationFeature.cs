using EasyDesk.CleanArchitecture.Application.Features;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;

public class AuthorizationFeature : IAppFeature
{
    private readonly Action<AuthorizationOptions> _configure;

    public AuthorizationFeature(Action<AuthorizationOptions> configure)
    {
        _configure = configure;
    }

    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        var options = new AuthorizationOptions(services, app);
        _configure(options);
    }
}

public static class AuthorizationFeatureExtensions
{
    public static AppBuilder AddAuthorization(this AppBuilder builder, Action<AuthorizationOptions> configure)
    {
        return builder.AddFeature(new AuthorizationFeature(configure));
    }

    public static bool HasAuthorization(this AppDescription app) => app.HasFeature<AuthorizationFeature>();
}
