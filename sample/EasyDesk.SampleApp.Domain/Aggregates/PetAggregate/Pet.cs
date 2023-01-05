using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Hydration;
using EasyDesk.CleanArchitecture.Domain.Model;

namespace EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;

public class Pet : AggregateRoot, IAggregateRootWithHydration<int>
{
    public Pet(int id, Name nickname, Guid personId)
    {
        Id = id;
        Nickname = nickname;
        PersonId = personId;
    }

    public int Id { get; private set; }

    public Name Nickname { get; }

    public Guid PersonId { get; private set; }

    public void ChangeOwner(Guid newOwnerId)
    {
        PersonId = newOwnerId;
    }

    public static Pet Create(Name nickname, Guid personId) =>
        new(id: 0, nickname, personId);

    public void Hydrate(int data) => Id = data;
}
