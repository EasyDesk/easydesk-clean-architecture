using EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;
using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Application.Dto;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;

namespace EasyDesk.SampleApp.Infrastructure.EfCore.Model;

public class PetModel : IEntityPersistence<Pet, PetModel>, IWithHydration<int>, IMultitenantEntity, IProjectable<PetModel, PetDto>
{
    public int Id { get; set; }

    required public string Nickname { get; set; }

    public Guid PersonId { get; set; }

    public string? Tenant { get; set; }

    public PersonModel Person { get; set; } = null!;

    public static Expression<Func<PetModel, PetDto>> Projection() => src =>
        new PetDto(src.Id, src.Nickname, src.PersonId);

    public Pet ToDomain() => new(Id, new Name(Nickname), PersonId);

    public int GetHydrationData() => Id;

    public static void ApplyChanges(Pet origin, PetModel destination)
    {
        destination.Id = origin.Id;
        destination.Nickname = origin.Nickname;
        destination.PersonId = origin.OwnerId;
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
