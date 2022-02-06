using System.Collections.Immutable;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public interface IPermissionsProvider
{
    Task<IImmutableSet<Permission>> GetPermissionsForUser(UserInfo userDescription);
}
