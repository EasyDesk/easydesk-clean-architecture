using EasyDesk.Tools.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using static EasyDesk.Tools.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

public class StaticRolesToPermissionsMapper : IRolesToPermissionsMapper
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
