using System.Collections.Immutable;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

public interface IPermissionsProvider
{
    Task<IImmutableSet<Permission>> GetPermissionsForUser(UserInfo userDescription);
}
