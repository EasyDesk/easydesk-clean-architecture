﻿using EasyDesk.CleanArchitecture.Dal.EfCore.Domain;
using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Model;

public class PetModel : IAggregateRootModel<Pet, PetModel>, IWithHydration<int>, IMultitenantEntity, IProjectable<PetModel, PetDto>
{
    public int Id { get; set; }

    public required string Nickname { get; set; }

    public Guid PersonId { get; set; }

    public string? Tenant { get; set; }

    public PersonModel Person { get; set; } = null!;

    public static Expression<Func<PetModel, PetDto>> Projection() => src =>
        new PetDto(src.Id, src.Nickname);

    public Pet ToDomain() => new(Id, new Name(Nickname), PersonId);

    public int GetHydrationData() => Id;

    public void ApplyChanges(Pet origin)
    {
        Id = origin.Id;
        Nickname = origin.Nickname;
        PersonId = origin.OwnerId;
    }

    public static PetModel ToPersistence(Pet origin) => new()
    {
        Id = origin.Id,
        Nickname = origin.Nickname,
        PersonId = origin.OwnerId,
    };

    internal class Configuration : IEntityTypeConfiguration<PetModel>
    {
        public void Configure(EntityTypeBuilder<PetModel> builder)
        {
            builder.HasKey(x => x.Id);

            SqlServerPropertyBuilderExtensions.UseHiLo(builder.Property(x => x.Id));
            NpgsqlPropertyBuilderExtensions.UseHiLo(builder.Property(x => x.Id));
        }
    }
}
