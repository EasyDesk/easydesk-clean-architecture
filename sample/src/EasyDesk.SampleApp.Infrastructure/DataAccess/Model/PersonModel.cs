using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.SampleApp.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Model
{
    public class PersonModel : IEntityWithinTenant
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool Married { get; set; }

        public string TenantId { get; set; }

        public class Configuration : IEntityTypeConfiguration<PersonModel>
        {
            public void Configure(EntityTypeBuilder<PersonModel> builder)
            {
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Name)
                    .IsRequired();
            }
        }

        public class MappingToSnapshot : DirectMapping<PersonModel, GetPeople.PersonSnapshot>
        {
            public MappingToSnapshot() : base(src => new(src.Id, src.Name, src.Married))
            {
            }
        }
    }
}
