using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Sagas;

internal class SagaModel : IMultitenantEntity
{
    public string Id { get; set; }

    public string Type { get; set; }

    public byte[] State { get; set; }

    public int Version { get; set; }

    public string TenantId { get; set; }

    public class Configuration : IEntityTypeConfiguration<SagaModel>
    {
        public void Configure(EntityTypeBuilder<SagaModel> builder)
        {
            builder.HasKey(x => new { x.Id, x.Type, x.TenantId });

            builder.Property(x => x.Id)
                .IsRequired();

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.State)
                .IsRequired();
        }
    }
}
