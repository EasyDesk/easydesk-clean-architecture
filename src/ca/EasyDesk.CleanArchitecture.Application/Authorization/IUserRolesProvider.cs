using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public interface IUserRolesProvider
{
    Task<IImmutableSet<Role>> GetRolesForUser(UserInfo userInfo);
}
