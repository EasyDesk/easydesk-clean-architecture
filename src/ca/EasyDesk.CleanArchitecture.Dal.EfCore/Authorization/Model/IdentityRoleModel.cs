using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model;

internal class IdentityRoleModel : IMultitenantEntity
{
    public required string Role { get; set; }

    public required string Realm { get; set; }

    public required string Identity { get; set; }

    public string? Tenant { get; set; }

    public string? TenantFk { get; set; }

    public static IdentityRoleModel Create(Realm realm, IdentityId id, string role) =>
        new()
        {
            Realm = realm,
            Identity = id,
            Role = role,
        };

    public sealed class Configuration : IEntityTypeConfiguration<IdentityRoleModel>
    {
        public void Configure(EntityTypeBuilder<IdentityRoleModel> builder)
        {
            builder.HasKey(x => new { x.Identity, x.Role, x.Tenant });

            builder.Property(x => x.Identity).HasMaxLength(IdentityId.MaxLength);

            builder.Property(x => x.Role)
                .HasMaxLength(Application.Authorization.Model.Role.MaxLength);

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
