using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public interface IIdentityRolesManager
{
    Task GrantRolesToIdentity(Identity identity, IEnumerable<Role> roles);

    Task RevokeRolesToIdentity(Identity identity, IEnumerable<Role> roles);

    Task RevokeAllRolesToIdentity(Identity identity);
}

public static class IdentityRolesManagerExtensions
{
    public static Task GrantRolesToIdentity(this IIdentityRolesManager manager, Identity identity, params Role[] roles) =>
        manager.GrantRolesToIdentity(identity, roles.AsEnumerable());

    public static Task RevokeRolesToIdentity(this IIdentityRolesManager manager, Identity identity, params Role[] roles) =>
        manager.RevokeRolesToIdentity(identity, roles.AsEnumerable());
}
