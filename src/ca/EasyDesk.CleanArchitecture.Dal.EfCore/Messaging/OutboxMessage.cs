using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;

internal class OutboxMessage
{
    public int Id { get; set; }

    required public byte[] Content { get; set; }

    required public byte[] Headers { get; set; }

    required public string DestinationAddress { get; set; }

    public class Configuration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.DestinationAddress)
                .IsRequired();
        }
    }
}
