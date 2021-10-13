namespace EasyDesk.CleanArchitecture.Domain.Model.Roles
{
    public record BasicRole(RoleId Id) : IRole
    {
        public static BasicRole From(string roleId) => new(RoleId.From(roleId));
    }
}
