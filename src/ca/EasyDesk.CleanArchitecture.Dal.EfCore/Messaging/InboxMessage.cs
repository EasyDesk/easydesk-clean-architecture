using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;

internal class InboxMessage
{
    public required string Id { get; set; }

    public sealed class Configuration : IEntityTypeConfiguration<InboxMessage>
    {
        public void Configure(EntityTypeBuilder<InboxMessage> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}
