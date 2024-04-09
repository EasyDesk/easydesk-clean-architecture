using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;

public sealed class AuthorizationOptions
{
    private Action<IServiceCollection, AppDescription> _configure;

    public AuthorizationOptions()
    {
        _configure = DoNotUsePermissionsBasedAuth;
    }

    private void DoNotUsePermissionsBasedAuth(IServiceCollection services, AppDescription app)
    {
        services.AddSingleton<IAgentPermissionsProvider, EmptyPermissionsProvider>();
    }

    public AuthorizationOptions RoleBased(Action<RoleBasedAuthorizationOptions> configure)
    {
        return Configure((services, app) =>
        {
            new RoleBasedAuthorizationOptions().Also(configure).Apply(services, app);
        });
    }

    public AuthorizationOptions Custom(Func<IServiceProvider, IAgentPermissionsProvider> factory, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        return Configure((services, app) =>
        {
            services.Add(new(typeof(IAgentPermissionsProvider), factory, lifetime));
        });
    }

    private AuthorizationOptions Configure(Action<IServiceCollection, AppDescription> configuration)
    {
        _configure = configuration;
        return this;
    }

    internal void Apply(IServiceCollection services, AppDescription app)
    {
        services.AddScoped(typeof(IStaticAuthorizer<>), typeof(DefaultStaticAuthorizer<>));
        services.AddScoped<IAuthorizationProvider, DefaultAuthorizationProvider>();
        services.Decorate<IAuthorizationProvider, CachedAuthorizationProvider>();
        _configure?.Invoke(services, app);
    }
}
