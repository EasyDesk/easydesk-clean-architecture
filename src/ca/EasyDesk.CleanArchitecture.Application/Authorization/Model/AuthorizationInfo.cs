using EasyDesk.CleanArchitecture.Application.ContextProvider;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Model;

public record AuthorizationInfo(UserInfo UserInfo, IImmutableSet<Permission> Permissions)
{
    public bool HasUserId(UserId validUserId) =>
        HasAnyUserIdAmong(validUserId);

    public bool HasAnyUserIdAmong(params UserId[] validUserIds) =>
        HasAnyUserIdAmong(validUserIds.AsEnumerable());

    public bool HasAnyUserIdAmong(IEnumerable<UserId> validUserIds) =>
        validUserIds.Contains(UserInfo.UserId);

    public bool HasPermission(Permission permission) =>
        HasAnyPermissionAmong(permission);

    public bool HasAnyPermissionAmong(params Permission[] validPermissions) =>
        HasAnyPermissionAmong(validPermissions.AsEnumerable());

    public bool HasAnyPermissionAmong(IEnumerable<Permission> validPermissions) =>
        Permissions.Overlaps(validPermissions);
}
