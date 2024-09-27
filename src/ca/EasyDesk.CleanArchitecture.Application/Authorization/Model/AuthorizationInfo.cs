using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Model;

public record AuthorizationInfo(Agent Agent, IFixedSet<Permission> Permissions)
{
    public bool HasRealm(Realm realm) =>
        Agent.Identities.ContainsKey(realm);

    public bool HasPermission(Permission permission) =>
        HasAnyPermissionAmong(permission);

    public bool HasAnyPermissionAmong(params Permission[] permissions) =>
        HasAnyPermissionAmong(permissions.AsEnumerable());

    public bool HasAnyPermissionAmong(IEnumerable<Permission> permissions) =>
        Permissions.Overlaps(permissions);
}
