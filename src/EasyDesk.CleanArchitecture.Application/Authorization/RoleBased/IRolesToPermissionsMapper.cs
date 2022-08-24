using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

public interface IRolesToPermissionsMapper
{
    Task<IImmutableSet<Permission>> MapRolesToPermissions(IEnumerable<Role> roles);
}
