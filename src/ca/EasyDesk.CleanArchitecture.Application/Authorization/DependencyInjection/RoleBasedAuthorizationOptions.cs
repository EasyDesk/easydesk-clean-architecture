using Autofac;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;

namespace EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;

public class RoleBasedAuthorizationOptions
{
    private Action<ServiceRegistry, AppDescription> _configureRolesProvider = default!;
    private Action<ServiceRegistry, AppDescription> _configurePermissionsMapper = default!;

    public RoleBasedAuthorizationOptions()
    {
        WithRolesManagement();
        WithStaticPermissions(x => { });
    }

    public RoleBasedAuthorizationOptions WithCustomRolesProvider(Func<IComponentContext, IAgentRolesProvider> factory)
    {
        _configureRolesProvider = (registry, _) => registry.ConfigureContainer(builder =>
        {
            builder.Register(factory)
                .As<IAgentRolesProvider>()
                .InstancePerDependency();
        });
        return this;
    }

    public RoleBasedAuthorizationOptions WithRolesManagement()
    {
        _configureRolesProvider = (registry, app) =>
        {
            app.RequireModule<DataAccessModule>().Implementation.AddRolesManagement(registry, app);
        };
        return this;
    }

    public RoleBasedAuthorizationOptions WithCustomPermissionsMapper(Func<IComponentContext, IRolesToPermissionsMapper> factory)
    {
        _configurePermissionsMapper = (registry, _) => registry.ConfigureContainer(builder =>
        {
            builder.Register(factory)
                .As<IRolesToPermissionsMapper>()
                .InstancePerDependency();
        });
        return this;
    }

    public RoleBasedAuthorizationOptions WithStaticPermissions(Action<StaticRolesToPermissionsBuilder> configure)
    {
        _configurePermissionsMapper = (registry, _) =>
        {
            registry.ConfigureContainer(builder =>
            {
                var rolesToPermissionsBuilder = new StaticRolesToPermissionsBuilder();
                configure(rolesToPermissionsBuilder);
                var rolesToPermissionsMapper = rolesToPermissionsBuilder.Build();
                builder.RegisterInstance(rolesToPermissionsMapper)
                    .As<IRolesToPermissionsMapper>()
                    .SingleInstance();
            });
        };
        return this;
    }

    internal void Apply(ServiceRegistry registry, AppDescription app)
    {
        _configureRolesProvider.Invoke(registry, app);
        _configurePermissionsMapper.Invoke(registry, app);

        registry.ConfigureContainer(builder =>
        {
            builder.RegisterType<RoleBasedPermissionsProvider>()
                .As<IAgentPermissionsProvider>()
                .InstancePerDependency();
        });
    }
}
