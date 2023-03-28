using EasyDesk.Commons.Collections;
using System.Collections.Immutable;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

internal class StaticRolesToPermissionsMapper : IRolesToPermissionsMapper
{
    private readonly IImmutableDictionary<Role, IImmutableSet<Permission>> _permissionsByRole;

    public StaticRolesToPermissionsMapper(IImmutableDictionary<Role, IImmutableSet<Permission>> permissionsByRole)
    {
        _permissionsByRole = permissionsByRole;
    }

    public Task<IImmutableSet<Permission>> MapRolesToPermissions(IEnumerable<Role> roles) =>
        Task.FromResult(ConvertRolesToPermissions(roles));

    private IImmutableSet<Permission> ConvertRolesToPermissions(IEnumerable<Role> roles)
    {
        return roles
            .SelectMany(r => _permissionsByRole.GetOption(r))
            .Flatten()
            .ToEquatableSet();
    }
}
