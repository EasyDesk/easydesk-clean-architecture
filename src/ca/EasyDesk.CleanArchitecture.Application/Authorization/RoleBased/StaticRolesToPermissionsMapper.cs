using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

internal class StaticRolesToPermissionsMapper : IRolesToPermissionsMapper
{
    private readonly IFixedMap<Role, IFixedSet<Permission>> _permissionsByRole;

    public StaticRolesToPermissionsMapper(IFixedMap<Role, IFixedSet<Permission>> permissionsByRole)
    {
        _permissionsByRole = permissionsByRole;
    }

    public Task<IFixedSet<Permission>> MapRolesToPermissions(IEnumerable<Role> roles) =>
        Task.FromResult(ConvertRolesToPermissions(roles));

    private IFixedSet<Permission> ConvertRolesToPermissions(IEnumerable<Role> roles)
    {
        return roles
            .SelectMany(r => _permissionsByRole.Get(r))
            .Flatten()
            .ToFixedSet();
    }
}
