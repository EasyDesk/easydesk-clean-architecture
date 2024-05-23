using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.Commons.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

public interface IRolesToPermissionsMapper
{
    Task<IFixedSet<Permission>> MapRolesToPermissions(IEnumerable<Role> roles);
}
