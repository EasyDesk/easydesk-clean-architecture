namespace EasyDesk.CleanArchitecture.Domain.Roles
{
    public record BasicRole(RoleId Id) : IRole
    {
        public static BasicRole From(string roleId) => new(RoleId.From(roleId));
    }
}
