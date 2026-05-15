using EasyDesk.CleanArchitecture.Application.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auth.Model;

internal class IdentityRoleModel
{
    public required string Role { get; set; }

    public required string Realm { get; set; }

    public required string Identity { get; set; }

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
            builder.HasKey(x => new { x.Identity, x.Role, });

            builder.Property(x => x.Identity).HasMaxLength(IdentityId.MaxLength);

            builder.Property(x => x.Role)
                .HasMaxLength(Application.Authorization.Model.Role.MaxLength);
        }
    }
}
