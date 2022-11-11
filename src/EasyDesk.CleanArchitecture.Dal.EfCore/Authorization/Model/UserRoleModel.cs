using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model;

internal class UserRoleModel : IMultitenantEntity
{
    public string RoleId { get; set; }

    public string UserId { get; set; }

    public string TenantId { get; set; }

    public static UserRoleModel Create(string userId, string roleId) =>
        new()
        {
            UserId = userId,
            RoleId = roleId
        };

    public class Configuration : IEntityTypeConfiguration<UserRoleModel>
    {
        public void Configure(EntityTypeBuilder<UserRoleModel> builder)
        {
            builder.HasKey(x => new { x.UserId, x.RoleId });

            builder.Property(x => x.RoleId)
                .IsRequired()
                .HasMaxLength(Role.MaxLength);

            builder.Property(x => x.UserId)
                .IsRequired();
        }
    }
}
