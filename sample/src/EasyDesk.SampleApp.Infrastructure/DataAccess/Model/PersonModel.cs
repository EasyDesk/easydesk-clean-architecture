using EasyDesk.CleanArchitecture.Dal.EfCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Model
{
    public class PersonModel : AggregateRootModel<Guid>
    {
        public string Name { get; set; }

        public bool Married { get; set; }

        public class Configuration : IEntityTypeConfiguration<PersonModel>
        {
            public void Configure(EntityTypeBuilder<PersonModel> builder)
            {
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Name)
                    .IsRequired();
            }
        }
    }
}
