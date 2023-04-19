using EasyDesk.CleanArchitecture.Application.ContextProvider;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

public interface IPermissionsProvider
{
    Task<IImmutableSet<Permission>> GetPermissionsForUser(UserInfo userInfo);
}
