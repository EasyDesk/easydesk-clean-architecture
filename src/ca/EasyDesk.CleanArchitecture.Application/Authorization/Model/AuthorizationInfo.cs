using EasyDesk.CleanArchitecture.Application.ContextProvider;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Model;

public record AuthorizationInfo(Identity Identity, IImmutableSet<Permission> Permissions)
{
    public bool HasId(IdentityId id) =>
        HasAnyIdAmong(id);

    public bool HasAnyIdAmong(params IdentityId[] ids) =>
        HasAnyIdAmong(ids.AsEnumerable());

    public bool HasAnyIdAmong(IEnumerable<IdentityId> ids) =>
        ids.Contains(Identity.Id);

    public bool HasPermission(Permission permission) =>
        HasAnyPermissionAmong(permission);

    public bool HasAnyPermissionAmong(params Permission[] permissions) =>
        HasAnyPermissionAmong(permissions.AsEnumerable());

    public bool HasAnyPermissionAmong(IEnumerable<Permission> permissions) =>
        Permissions.Overlaps(permissions);
}
