namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

public interface IUserRolesManager : IUserRolesProvider
{
    Task GrantRolesToUser(UserInfo userInfo, IEnumerable<Role> roles);

    Task RevokeRolesToUser(UserInfo userInfo, IEnumerable<Role> roles);
}

public static class UserRolesManagerExtensions
{
    public static Task GrantRolesToUser(this IUserRolesManager manager, UserInfo userInfo, params Role[] roles) =>
        manager.GrantRolesToUser(userInfo, roles.AsEnumerable());

    public static Task RevokeRolesToUser(this IUserRolesManager manager, UserInfo userInfo, params Role[] roles) =>
        manager.RevokeRolesToUser(userInfo, roles.AsEnumerable());
}
