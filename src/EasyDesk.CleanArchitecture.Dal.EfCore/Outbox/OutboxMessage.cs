using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; }

    public string Content { get; set; }

    public string Type { get; set; }

    public string TenantId { get; set; }

    public Timestamp Timestamp { get; set; }

    public class Configuration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Timestamp)
                .IsRequired();
        }
    }
}
