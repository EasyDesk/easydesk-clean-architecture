using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public interface IUserPermissionsProvider
{
    Task<IImmutableSet<Permission>> GetPermissionsForUser(UserInfo userInfo);
}
