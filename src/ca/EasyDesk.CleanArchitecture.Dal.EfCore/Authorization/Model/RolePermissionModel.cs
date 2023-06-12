using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model;

internal class RolePermissionModel : IMultitenantEntity
{
    required public string RoleId { get; set; }

    required public string PermissionName { get; set; }

    public string? Tenant { get; set; }

    public sealed class Configuration : IEntityTypeConfiguration<RolePermissionModel>
    {
        public void Configure(EntityTypeBuilder<RolePermissionModel> builder)
        {
            builder.HasKey(x => new { x.RoleId, x.PermissionName, x.Tenant });

            builder.Property(x => x.RoleId)
                .HasMaxLength(Role.MaxLength);

            builder.Property(x => x.PermissionName)
                .HasMaxLength(Permission.MaxLength);

            builder.HasOne<TenantModel>()
                .WithMany()
                .HasForeignKey(x => x.Tenant)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
