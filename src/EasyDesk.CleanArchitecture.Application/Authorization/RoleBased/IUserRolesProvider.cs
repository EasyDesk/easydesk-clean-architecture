using System.Collections.Immutable;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

public interface IUserRolesProvider
{
    Task<IImmutableSet<Role>> GetRolesForUser(UserInfo userInfo);
}
