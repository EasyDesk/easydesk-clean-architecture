using EasyDesk.CleanArchitecture.Application.ContextProvider;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Model;

public record AuthorizationInfo(Agent Agent, IImmutableSet<Permission> Permissions)
{
    public bool HasPermission(Permission permission) =>
        HasAnyPermissionAmong(permission);

    public bool HasAnyPermissionAmong(params Permission[] permissions) =>
        HasAnyPermissionAmong(permissions.AsEnumerable());

    public bool HasAnyPermissionAmong(IEnumerable<Permission> permissions) =>
        Permissions.Overlaps(permissions);
}
