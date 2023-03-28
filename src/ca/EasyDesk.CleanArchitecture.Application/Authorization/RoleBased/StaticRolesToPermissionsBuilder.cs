using System.Collections.Immutable;
using System.Data;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

public class StaticRolesToPermissionsBuilder
{
    private IImmutableSet<(Role, Permission)> _rolePermissionPairs = Set<(Role, Permission)>();

    public RolesSpecification ForRoles(IEnumerable<Role> roles) => new(this, roles);

    public RolesSpecification ForRoles(params Role[] roles) => ForRoles(roles.AsEnumerable());

    public PermissionsSpecification ForPermissions(IEnumerable<Permission> permissions) => new(this, permissions);

    public PermissionsSpecification ForPermissions(params Permission[] permissions) => ForPermissions(permissions.AsEnumerable());

    public StaticRolesToPermissionsBuilder AddRolePermissionPairs(params (Role, Permission)[] pairs) =>
        AddRolePermissionPairs(pairs.AsEnumerable());

    public StaticRolesToPermissionsBuilder AddRolePermissionPairs(IEnumerable<(Role, Permission)> pairs)
    {
        _rolePermissionPairs = _rolePermissionPairs.Union(pairs);
        return this;
    }

    internal StaticRolesToPermissionsMapper Build()
    {
        var permissionsByRole = _rolePermissionPairs
            .GroupBy(pair => pair.Item1, (role, pairs) => (role, pairs.Select(p => p.Item2).ToEquatableSet()))
            .ToEquatableMap();
        return new StaticRolesToPermissionsMapper(permissionsByRole);
    }
}

public class PermissionsSpecification
{
    private readonly StaticRolesToPermissionsBuilder _builder;
    private readonly IEnumerable<Permission> _permissions;

    public PermissionsSpecification(StaticRolesToPermissionsBuilder builder, IEnumerable<Permission> permissions)
    {
        _builder = builder;
        _permissions = permissions;
    }

    public void AddRoles(params Role[] roles) => AddRoles(roles.AsEnumerable());

    public void AddRoles(IEnumerable<Role> roles)
    {
        var pairs = from role in roles
                    from permission in _permissions
                    select (role, permission);

        _builder.AddRolePermissionPairs(pairs);
    }
}

public class RolesSpecification
{
    private readonly StaticRolesToPermissionsBuilder _builder;
    private readonly IEnumerable<Role> _roles;

    public RolesSpecification(StaticRolesToPermissionsBuilder builder, IEnumerable<Role> roles)
    {
        _builder = builder;
        _roles = roles;
    }

    public void AddPermissions(params Permission[] permissions) => AddPermissions(permissions.AsEnumerable());

    public void AddPermissions(IEnumerable<Permission> permissions)
    {
        var pairs = from role in _roles
                    from permission in permissions
                    select (role, permission);

        _builder.AddRolePermissionPairs(pairs);
    }
}
