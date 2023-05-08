using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;

namespace EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;

public interface IPetRepository :
    IFindByIdRepository<Pet, int>,
    ISaveAndHydrateRepository<Pet, int>,
    IRemoveRepository<Pet>
{
    Task RemoveAll();
}
