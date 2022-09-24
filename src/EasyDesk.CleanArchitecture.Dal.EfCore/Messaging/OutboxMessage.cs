using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;

public class OutboxMessage
{
    public int Id { get; set; }

    public byte[] Content { get; set; }

    public byte[] Headers { get; set; }

    public string DestinationAddress { get; set; }

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
