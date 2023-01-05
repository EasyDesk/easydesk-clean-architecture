using EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess.Model;

public class PetModel : IPersistenceModelWithHydration<Pet, PetModel, int>, IMultitenantEntity, IProjectable<PetModel, PetSnapshot>
{
    public int Id { get; set; }

    public string Nickname { get; set; }

    public Guid PersonId { get; set; }

    public string TenantId { get; set; }

    public PersonModel Person { get; set; }

    public static Expression<Func<PetModel, PetSnapshot>> Projection() =>
        src => new(src.Id, src.Nickname, src.PersonId);

    public Pet ToDomain() => new(Id, Name.From(Nickname), PersonId);

    public int GetHydrationData() => Id;

    public static PetModel CreateDefaultPersistenceModel() => new();

    public static void ApplyChanges(Pet origin, PetModel destination)
    {
        destination.Id = origin.Id;
        destination.Nickname = origin.Nickname;
        destination.PersonId = origin.OwnerId;
    }

    public class Configuration : IEntityTypeConfiguration<PetModel>
    {
        public void Configure(EntityTypeBuilder<PetModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .UseHiLo();

            builder.Property(x => x.Nickname)
                .IsRequired();
        }
    }
}
