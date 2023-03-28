using EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased.DependencyInjection;

public class RoleBasedAuthorizationOptions
{
    private readonly IServiceCollection _services;
    private readonly AppDescription _app;

    public RoleBasedAuthorizationOptions(IServiceCollection services, AppDescription app)
    {
        _services = services;
        _app = app;
    }

    public RoleBasedAuthorizationOptions WithDataAccessPermissions()
    {
        _app.RequireModule<DataAccessModule>().Implementation.AddRoleBasedPermissionsProvider(_services, _app);
        return this;
    }

    public RoleBasedAuthorizationOptions WithStaticPermissions(Action<StaticRolesToPermissionsBuilder> configure)
    {
        var builder = new StaticRolesToPermissionsBuilder();
        configure(builder);
        var rolesToPermissionsMapper = builder.Build();

        _services.AddSingleton<IRolesToPermissionsMapper>(rolesToPermissionsMapper);
        _app.RequireModule<DataAccessModule>().Implementation.AddRoleManager(_services, _app);
        _services.AddScoped<IPermissionsProvider, RoleBasedPermissionsProvider>();
        return this;
    }
}

public static class RoleBasedExtensions
{
    public static RoleBasedAuthorizationOptions UseRoleBasedPermissions(this AuthorizationOptions options)
    {
        options.Services.AddScoped(typeof(IAuthorizer), typeof(RoleBasedAuthorizer));
        return new(options.Services, options.App);
    }
}
