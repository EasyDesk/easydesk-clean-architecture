using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model;

internal class RolePermissionModel : IMultitenantEntity
{
    public string? RoleId { get; set; }

    public string? PermissionName { get; set; }

    public string? TenantId { get; set; }

    public class Configuration : IEntityTypeConfiguration<RolePermissionModel>
    {
        public void Configure(EntityTypeBuilder<RolePermissionModel> builder)
        {
            builder.HasKey(x => new { x.RoleId, x.PermissionName, x.TenantId });

            builder.Property(x => x.RoleId)
                .IsRequired()
                .HasMaxLength(Role.MaxLength);

            builder.Property(x => x.PermissionName)
                .IsRequired()
                .HasMaxLength(Permission.MaxLength);

            builder.HasOne<TenantModel>()
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
