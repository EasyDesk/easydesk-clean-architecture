using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Hydration;
using EasyDesk.CleanArchitecture.Domain.Model;

namespace EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;

public class Pet : AggregateRoot, IAggregateRootWithHydration<int>
{
    public Pet(int id, Name nickname, Guid ownerId)
    {
        Id = id;
        Nickname = nickname;
        OwnerId = ownerId;
    }

    public int Id { get; private set; }

    public Name Nickname { get; }

    public Guid OwnerId { get; private set; }

    public void ChangeOwner(Guid newOwnerId)
    {
        OwnerId = newOwnerId;
    }

    public static Pet Create(Name nickname, Guid personId) =>
        new(id: 0, nickname, personId);

    public void Hydrate(int data) => Id = data;
}
