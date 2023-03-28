using EasyDesk.CleanArchitecture.Application.ContextProvider;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

public interface IUserRolesProvider
{
    Task<IImmutableSet<Role>> GetRolesForUser(UserInfo userInfo);
}
