using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;

internal class InboxMessage
{
    public required string Id { get; set; }

    public required Instant Instant { get; set; }

    public sealed class Configuration : IEntityTypeConfiguration<InboxMessage>
    {
        public void Configure(EntityTypeBuilder<InboxMessage> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}
