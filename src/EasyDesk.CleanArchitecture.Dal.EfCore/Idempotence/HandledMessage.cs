using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Idempotence;

public class HandledMessage
{
    public string Id { get; set; }

    public class Configuration : IEntityTypeConfiguration<HandledMessage>
    {
        public void Configure(EntityTypeBuilder<HandledMessage> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired();
        }
    }
}
