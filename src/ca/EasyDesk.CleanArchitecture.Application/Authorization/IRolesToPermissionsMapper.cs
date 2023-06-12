using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public interface IRolesToPermissionsMapper
{
    Task<IImmutableSet<Permission>> MapRolesToPermissions(IEnumerable<Role> roles);
}
