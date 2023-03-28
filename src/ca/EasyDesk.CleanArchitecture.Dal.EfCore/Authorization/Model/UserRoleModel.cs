using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantIdType = EasyDesk.CleanArchitecture.Application.Multitenancy.TenantId;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model;

internal class UserRoleModel : IMultitenantEntity
{
    required public string RoleId { get; set; }

    required public string UserId { get; set; }

    public string? TenantId { get; set; }

    public string? TenantIdFk { get; set; }

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
            builder.HasKey(x => new { x.UserId, x.RoleId, x.TenantId });

            builder.Property(x => x.RoleId)
                .IsRequired()
                .HasMaxLength(Role.MaxLength);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.TenantIdFk)
                .IsRequired(false)
                .HasMaxLength(TenantIdType.MaxLength)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<TenantIdFkGenerator>();

            builder.HasOne<TenantModel>()
                .WithMany()
                .HasForeignKey(x => x.TenantIdFk)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class TenantIdFkGenerator : AuthorizationContext.TenantIdGenerator
    {
        protected override object? NextValue(EntityEntry entry)
        {
            var currentTenantAsString = base.NextValue(entry);
            return currentTenantAsString is AuthorizationContext.PublicTenantName
                ? null
                : currentTenantAsString;
        }
    }
}
