using Autofac;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;

namespace EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;

public sealed class AuthorizationOptions
{
    private Action<ServiceRegistry, AppDescription> _configure;

    public AuthorizationOptions()
    {
        _configure = DoNotUsePermissionsBasedAuth;
    }

    private void DoNotUsePermissionsBasedAuth(ServiceRegistry registry, AppDescription app)
    {
        registry.ConfigureContainer(builder =>
        {
            builder.RegisterType<EmptyPermissionsProvider>()
                .As<IAgentPermissionsProvider>()
                .SingleInstance();
        });
    }

    public AuthorizationOptions RoleBased(Action<RoleBasedAuthorizationOptions> configure)
    {
        return Configure((registry, app) =>
        {
            new RoleBasedAuthorizationOptions().Also(configure).Apply(registry, app);
        });
    }

    public AuthorizationOptions Custom(Func<IComponentContext, IAgentPermissionsProvider> factory)
    {
        return Configure((registry, app) => registry.ConfigureContainer(builder =>
        {
            builder.Register(factory)
                .As<IAgentPermissionsProvider>()
                .InstancePerDependency();
        }));
    }

    private AuthorizationOptions Configure(Action<ServiceRegistry, AppDescription> configuration)
    {
        _configure = configuration;
        return this;
    }

    internal void Apply(ServiceRegistry registry, AppDescription app)
    {
        registry.ConfigureContainer(builder =>
        {
            builder.RegisterGeneric(typeof(DefaultStaticAuthorizer<>))
                .As(typeof(IStaticAuthorizer<>))
                .InstancePerLifetimeScope();

            builder.RegisterType<DefaultAuthorizationProvider>()
                .As<IAuthorizationProvider>()
                .InstancePerLifetimeScope();

            builder.RegisterDecorator<CachedAuthorizationProvider, IAuthorizationProvider>();
        });

        _configure?.Invoke(registry, app);
    }
}
