using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.SampleApp.Application.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Model;

public class PersonModel : IMultitenantEntity
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public LocalDate DateOfBirth { get; set; }

    public string TenantId { get; set; }

    public class Configuration : IEntityTypeConfiguration<PersonModel>
    {
        public void Configure(EntityTypeBuilder<PersonModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FirstName)
                .IsRequired();

            builder.Property(x => x.LastName)
                .IsRequired();
        }
    }

    public class MappingToSnapshot : DirectMapping<PersonModel, PersonSnapshot>
    {
        public MappingToSnapshot() : base(src => new(src.Id, src.FirstName, src.LastName, src.DateOfBirth))
        {
        }
    }
}
