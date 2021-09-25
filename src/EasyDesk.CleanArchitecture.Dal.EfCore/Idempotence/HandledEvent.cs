using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Idempotence
{
    public class HandledEvent
    {
        public Guid Id { get; set; }

        public class Configuration : IEntityTypeConfiguration<HandledEvent>
        {
            public void Configure(EntityTypeBuilder<HandledEvent> builder)
            {
                builder.HasKey(x => x.Id);
            }
        }
    }
}
