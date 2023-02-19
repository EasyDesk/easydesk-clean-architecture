using EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.SoftDeletion;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Application.Snapshots;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;
using System.Linq.Expressions;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Model;

public class PersonModel : IMultitenantEntity, ISoftDeletable, IProjectable<PersonModel, PersonSnapshot>, IEntityPersistence<Person, PersonModel>
{
    public Guid Id { get; set; }

    required public string FirstName { get; set; }

    required public string LastName { get; set; }

    public LocalDate DateOfBirth { get; set; }

    public string? TenantId { get; set; }

    public bool IsDeleted { get; set; }

    required public string CreatedBy { get; set; }

    public ICollection<PetModel> Pets { get; set; } = new HashSet<PetModel>();

    required public AddressModel Residence { get; set; }

    public static Expression<Func<PersonModel, PersonSnapshot>> Projection() => src =>
        new(src.Id, src.FirstName, src.LastName, src.DateOfBirth, src.CreatedBy, src.Residence.ToProjection());

    public Person ToDomain() => new(Id, new Name(FirstName), new Name(LastName), DateOfBirth, AdminId.From(CreatedBy), Residence.ToDomain());

    public static void ApplyChanges(Person origin, PersonModel destination)
    {
        destination.Id = origin.Id;
        destination.FirstName = origin.FirstName;
        destination.LastName = origin.LastName;
        destination.DateOfBirth = origin.DateOfBirth;
        destination.CreatedBy = origin.CreatedBy;
    }

    public static PersonModel ToPersistence(Person origin) => new()
    {
        Id = origin.Id,
        FirstName = origin.FirstName,
        LastName = origin.LastName,
        CreatedBy = origin.CreatedBy,
        DateOfBirth = origin.DateOfBirth,
        Residence = AddressModel.ToPersistence(origin.Residence),
    };

    public class Configuration : IEntityTypeConfiguration<PersonModel>
    {
        public void Configure(EntityTypeBuilder<PersonModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FirstName)
                .IsRequired();

            builder.Property(x => x.LastName)
                .IsRequired();

            builder.HasMany(x => x.Pets)
                .WithOne(x => x.Person)
                .HasForeignKey(x => x.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.Residence).HasColumnType("jsonb");
        }
    }
}
