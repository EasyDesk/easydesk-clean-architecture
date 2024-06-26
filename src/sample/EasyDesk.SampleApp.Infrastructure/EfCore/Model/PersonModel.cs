﻿using EasyDesk.CleanArchitecture.Dal.EfCore.Domain;
using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;
using System.Linq.Expressions;

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Model;

public class PersonModel : IMultitenantEntity, IProjectable<PersonModel, PersonDto>, IAggregateRootModel<Person, PersonModel>
{
    public Guid Id { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public LocalDate DateOfBirth { get; set; }

    public string? Tenant { get; set; }

    public required string CreatedBy { get; set; }

    public ICollection<PetModel> Pets { get; set; } = new HashSet<PetModel>();

    public required AddressModel Residence { get; set; }

    public required bool Approved { get; set; }

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

    public void ApplyChanges(Person origin)
    {
        Id = origin.Id;
        FirstName = origin.FirstName;
        LastName = origin.LastName;
        DateOfBirth = origin.DateOfBirth;
        CreatedBy = origin.CreatedBy;
        Approved = origin.Approved;
        Residence.ApplyChanges(origin.Residence);
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
