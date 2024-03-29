﻿using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
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

    public AuthorizationOptions WithDynamicPermissions()
    {
        _configure = (services, app) =>
        {
            app.RequireModule<DataAccessModule>().Implementation.AddRolesManagement(services, app);
            app.RequireModule<DataAccessModule>().Implementation.AddPermissionsProvider(services, app);
        };
        return this;
    }

    public AuthorizationOptions WithStaticPermissions(Action<StaticRolesToPermissionsBuilder> configure)
    {
        _configure = (services, app) =>
        {
            var builder = new StaticRolesToPermissionsBuilder();
            configure(builder);
            var rolesToPermissionsMapper = builder.Build();

            app.RequireModule<DataAccessModule>().Implementation.AddRolesManagement(services, app);
            services.AddSingleton<IRolesToPermissionsMapper>(rolesToPermissionsMapper);
            services.AddScoped<IAgentPermissionsProvider, DefaultAgentPermissionsProvider>();
            services.AddScoped<IAuthorizationProvider, DefaultAuthorizationProvider>();
        };
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
