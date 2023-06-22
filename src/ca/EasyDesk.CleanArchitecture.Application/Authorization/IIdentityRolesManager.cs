using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public interface IIdentityRolesManager
{
    Task GrantRoles(Realm realm, IdentityId id, IEnumerable<Role> roles);

    Task RevokeRoles(Realm realm, IdentityId id, IEnumerable<Role> roles);

    Task RevokeAllRoles(Realm realm, IdentityId id);
}

public static class IdentityRolesManagerExtensions
{
    public static Task GrantRoles(this IIdentityRolesManager manager, Realm realm, IdentityId id, params Role[] roles) =>
        manager.GrantRoles(realm, id, roles.AsEnumerable());

    public static Task RevokeRoles(this IIdentityRolesManager manager, Realm realm, IdentityId id, params Role[] roles) =>
        manager.RevokeRoles(realm, id, roles.AsEnumerable());

    public static Task GrantRoles(this IIdentityRolesManager manager, Identity identity, IEnumerable<Role> roles) =>
        manager.GrantRoles(identity.Realm, identity.Id, roles);

    public static Task GrantRoles(this IIdentityRolesManager manager, Identity identity, params Role[] roles) =>
        manager.GrantRoles(identity.Realm, identity.Id, roles);

    public static Task RevokeRoles(this IIdentityRolesManager manager, Identity identity, IEnumerable<Role> roles) =>
        manager.RevokeRoles(identity.Realm, identity.Id, roles);

    public static Task RevokeRoles(this IIdentityRolesManager manager, Identity identity, params Role[] roles) =>
        manager.RevokeRoles(identity.Realm, identity.Id, roles);

    public static Task RevokeAllRoles(this IIdentityRolesManager manager, Identity identity) =>
        manager.RevokeAllRoles(identity.Realm, identity.Id);
}
