using EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;
using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Application.Dto;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;
using System.Linq.Expressions;

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Model;

public class PersonModel : IMultitenantEntity, IProjectable<PersonModel, PersonDto>, IEntityPersistence<Person, PersonModel>
{
    public Guid Id { get; set; }

    required public string FirstName { get; set; }

    required public string LastName { get; set; }

    public LocalDate DateOfBirth { get; set; }

    public string? Tenant { get; set; }

    required public string CreatedBy { get; set; }

    public ICollection<PetModel> Pets { get; set; } = new HashSet<PetModel>();

    required public AddressModel Residence { get; set; }

    required public bool Approved { get; set; }

    public static Expression<Func<PersonModel, PersonDto>> Projection() => src => new PersonDto(
        src.Id,
        src.FirstName,
        src.LastName,
        src.DateOfBirth,
        src.CreatedBy,
        new(
            src.Residence.StreetName,
            src.Residence.StreetType.AsOption(),
            src.Residence.StreetNumber.AsOption(),
            src.Residence.City.AsOption(),
            src.Residence.District.AsOption(),
            src.Residence.Province.AsOption(),
            src.Residence.Region.AsOption(),
            src.Residence.State.AsOption(),
            src.Residence.Country.AsOption()),
        src.Approved);

    public Person ToDomain() => new(Id, new Name(FirstName), new Name(LastName), DateOfBirth, AdminId.From(CreatedBy), Residence.ToDomain(), Approved);

    public static void ApplyChanges(Person origin, PersonModel destination)
    {
        destination.Id = origin.Id;
        destination.FirstName = origin.FirstName;
        destination.LastName = origin.LastName;
        destination.DateOfBirth = origin.DateOfBirth;
        destination.CreatedBy = origin.CreatedBy;
        destination.Approved = origin.Approved;
        AddressModel.ApplyChanges(origin.Residence, destination.Residence);
    }

    public static PersonModel ToPersistence(Person origin) => new()
    {
        Id = origin.Id,
        FirstName = origin.FirstName,
        LastName = origin.LastName,
        CreatedBy = origin.CreatedBy,
        DateOfBirth = origin.DateOfBirth,
        Residence = AddressModel.ToPersistence(origin.Residence),
        Approved = origin.Approved,
    };

    internal class Configuration : IEntityTypeConfiguration<PersonModel>
    {
        public void Configure(EntityTypeBuilder<PersonModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasMany(x => x.Pets)
                .WithOne(x => x.Person)
                .HasForeignKey(x => x.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.OwnsOne(x => x.Residence, builder =>
            {
                builder.WithOwner();
                builder.Property(x => x.StreetType)
                    .HasMaxLength(PlaceName.MaxLength);
                builder.Property(x => x.StreetName)
                    .HasMaxLength(PlaceName.MaxLength);
                builder.Property(x => x.StreetNumber)
                    .HasMaxLength(PlaceName.MaxLength);
                builder.Property(x => x.City)
                    .HasMaxLength(PlaceName.MaxLength);
                builder.Property(x => x.District)
                    .HasMaxLength(PlaceName.MaxLength);
                builder.Property(x => x.Province)
                    .HasMaxLength(PlaceName.MaxLength);
                builder.Property(x => x.Region)
                    .HasMaxLength(PlaceName.MaxLength);
                builder.Property(x => x.State)
                    .HasMaxLength(PlaceName.MaxLength);
                builder.Property(x => x.Country)
                    .HasMaxLength(PlaceName.MaxLength);
            });
        }
    }
}
