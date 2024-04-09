using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;

public class RoleBasedAuthorizationOptions
{
    private Action<IServiceCollection, AppDescription> _configureRolesProvider = default!;
    private Action<IServiceCollection, AppDescription> _configurePermissionsMapper = default!;

    public RoleBasedAuthorizationOptions()
    {
        WithRolesManagement();
        WithStaticPermissions(x => { });
    }

    public RoleBasedAuthorizationOptions WithCustomRolesProvider(Func<IServiceProvider, IAgentRolesProvider> factory, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        _configureRolesProvider = (services, _) => services.Add(new(typeof(IAgentRolesProvider), factory, lifetime));
        return this;
    }

    public RoleBasedAuthorizationOptions WithRolesManagement()
    {
        _configureRolesProvider = (services, app) =>
        {
            app.RequireModule<DataAccessModule>().Implementation.AddRolesManagement(services, app);
        };
        return this;
    }

    public RoleBasedAuthorizationOptions WithCustomPermissionsMapper(Func<IServiceProvider, IRolesToPermissionsMapper> factory, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        _configurePermissionsMapper = (services, _) => services.Add(new(typeof(IRolesToPermissionsMapper), factory, lifetime));
        return this;
    }

    public RoleBasedAuthorizationOptions WithStaticPermissions(Action<StaticRolesToPermissionsBuilder> configure)
    {
        _configurePermissionsMapper = (services, _) =>
        {
            var builder = new StaticRolesToPermissionsBuilder();
            configure(builder);
            var rolesToPermissionsMapper = builder.Build();
            services.AddSingleton<IRolesToPermissionsMapper>(rolesToPermissionsMapper);
        };
        return this;
    }

    internal void Apply(IServiceCollection services, AppDescription app)
    {
        _configureRolesProvider.Invoke(services, app);
        _configurePermissionsMapper.Invoke(services, app);
        services.AddTransient<IAgentPermissionsProvider, RoleBasedPermissionsProvider>();
    }
}
