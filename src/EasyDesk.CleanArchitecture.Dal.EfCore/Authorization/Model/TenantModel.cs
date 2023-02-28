using EasyDesk.CleanArchitecture.Application.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Authorization.Model;

public class TenantModel
{
    required public string Id { get; set; }

    public class Configuration : IEntityTypeConfiguration<TenantModel>
    {
        public void Configure(EntityTypeBuilder<TenantModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasMaxLength(TenantId.MaxLength);
        }
    }
}
