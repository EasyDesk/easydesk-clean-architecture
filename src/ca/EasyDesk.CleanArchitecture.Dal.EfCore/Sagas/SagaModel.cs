using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Sagas;

internal class SagaModel : IMultitenantEntity
{
    required public string Id { get; set; }

    required public string Type { get; set; }

    public byte[] State { get; set; } = Array.Empty<byte>();

    public int? Version { get; set; }

    public string? TenantId { get; set; }

    public sealed class Configuration : IEntityTypeConfiguration<SagaModel>
    {
        public void Configure(EntityTypeBuilder<SagaModel> builder)
        {
            builder.HasKey(x => new { x.Id, x.Type, x.TenantId });
        }
    }
}
