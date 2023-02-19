using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;

namespace EasyDesk.SampleApp.Application.Snapshots;

public record PetSnapshot(int Id, string Nickname, Guid PersonId) : ISnapshot<PetSnapshot, Pet>
{
    public static PetSnapshot MapFrom(Pet src) => new(
        Id: src.Id,
        Nickname: src.Nickname,
        PersonId: src.OwnerId);
}
