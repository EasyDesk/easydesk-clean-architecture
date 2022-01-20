using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model;

public class RolePermissionModel
{
    public string RoleId { get; set; }

    public string PermissionName { get; set; }

    public class Configuration : IEntityTypeConfiguration<RolePermissionModel>
    {
        public void Configure(EntityTypeBuilder<RolePermissionModel> builder)
        {
            builder.HasKey(x => new { x.RoleId, x.PermissionName });

            builder.Property(x => x.RoleId)
                .IsRequired()
                .HasMaxLength(Role.MaxLength);

            builder.Property(x => x.PermissionName)
                .IsRequired()
                .HasMaxLength(Permission.MaxLength);
        }
    }
}
