using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;

namespace EasyDesk.SampleApp.Application.Queries;

public record PetSnapshot(int Id, string Nickname, Guid PersonId)
{
    public static PetSnapshot FromPet(Pet pet) => new(
        Id: pet.Id,
        Nickname: pet.Nickname,
        PersonId: pet.OwnerId);
}
