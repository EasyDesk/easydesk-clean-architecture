using EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;
using System.Linq.Expressions;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Model;

public class PersonModel : IMultitenantEntity, IProjectable<PersonModel, PersonSnapshot>, IPersistenceModel<Person, PersonModel>
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public LocalDate DateOfBirth { get; set; }

    public string TenantId { get; set; }

    public static Expression<Func<PersonModel, PersonSnapshot>> Projection() =>
        src => new(src.Id, src.FirstName, src.LastName, src.DateOfBirth);

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

    public Person ToDomain() => new(Id, Name.From(FirstName), Name.From(LastName), DateOfBirth);

    public static PersonModel CreateDefaultPersistenceModel() => new();

    public static void ApplyChanges(Person origin, PersonModel destination)
    {
        destination.Id = origin.Id;
        destination.FirstName = origin.FirstName;
        destination.LastName = origin.LastName;
        destination.DateOfBirth = origin.DateOfBirth;
    }
}
