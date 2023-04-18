using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model;

internal class UserRoleModel : IMultitenantEntity
{
    required public string Role { get; set; }

    required public string User { get; set; }

    public string? Tenant { get; set; }

    public string? TenantFk { get; set; }

    public static UserRoleModel Create(string userId, string roleId) =>
        new()
        {
            User = userId,
            Role = roleId
        };

    public sealed class Configuration : IEntityTypeConfiguration<UserRoleModel>
    {
        public void Configure(EntityTypeBuilder<UserRoleModel> builder)
        {
            builder.HasKey(x => new { x.User, x.Role, x.Tenant });

            builder.Property(x => x.User).HasMaxLength(UserId.MaxLength);

            builder.Property(x => x.Role)
                .HasMaxLength(Application.Authorization.RoleBased.Role.MaxLength);

            builder.Property(x => x.TenantFk)
                .IsRequired(false)
                .HasMaxLength(TenantId.MaxLength)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<TenantIdFkGenerator>();

            builder.HasOne<TenantModel>()
                .WithMany()
                .HasForeignKey(x => x.TenantFk)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class TenantIdFkGenerator : AbstractDbContext.TenantIdGenerator
    {
        protected override object? NextValue(EntityEntry entry)
        {
            var currentTenantAsString = base.NextValue(entry);
            return currentTenantAsString is AbstractDbContext.PublicTenantName
                ? null
                : currentTenantAsString;
        }
    }
}
