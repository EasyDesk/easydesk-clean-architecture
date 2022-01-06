namespace EasyDesk.CleanArchitecture.Domain.Model.Roles;

public record BasicRole(RoleId Id) : IRole
{
    public static BasicRole From(string rawRoleId) => From(RoleId.From(rawRoleId));

    public static BasicRole From(RoleId roleId) => new(roleId);
}
