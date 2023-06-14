using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public interface IIdentityRolesManager
{
    Task GrantRolesToIdentity(IdentityId id, IEnumerable<Role> roles);

    Task RevokeRolesToIdentity(IdentityId id, IEnumerable<Role> roles);

    Task RevokeAllRolesToIdentity(IdentityId id);
}

public static class IdentityRolesManagerExtensions
{
    public static Task GrantRolesToIdentity(this IIdentityRolesManager manager, IdentityId id, params Role[] roles) =>
        manager.GrantRolesToIdentity(id, roles.AsEnumerable());

    public static Task RevokeRolesToIdentity(this IIdentityRolesManager manager, IdentityId id, params Role[] roles) =>
        manager.RevokeRolesToIdentity(id, roles.AsEnumerable());
}
