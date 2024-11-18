using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;

internal class OutboxMessage
{
    public int Id { get; set; }

    public required byte[] Content { get; set; }

    public required string Headers { get; set; }

    public byte[] Headers_Old { get; set; } = [];

    public required string DestinationAddress { get; set; }

    public sealed class Configuration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}
