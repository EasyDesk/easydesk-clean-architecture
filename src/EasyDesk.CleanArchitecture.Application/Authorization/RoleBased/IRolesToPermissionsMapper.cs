using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

public interface IRolesToPermissionsMapper
{
    Task<IImmutableSet<Permission>> MapRolesToPermissions(IEnumerable<Role> roles);
}
