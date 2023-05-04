using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

public interface IUserRolesManager
{
    Task GrantRolesToUser(UserId userId, IEnumerable<Role> roles);

    Task RevokeRolesToUser(UserId userId, IEnumerable<Role> roles);

    Task RevokeAllRolesToUser(UserId userId);
}

public static class UserRolesManagerExtensions
{
    public static Task GrantRolesToUser(this IUserRolesManager manager, UserId userId, params Role[] roles) =>
        manager.GrantRolesToUser(userId, roles.AsEnumerable());

    public static Task RevokeRolesToUser(this IUserRolesManager manager, UserId userId, params Role[] roles) =>
        manager.RevokeRolesToUser(userId, roles.AsEnumerable());
}
