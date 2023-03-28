using EasyDesk.CleanArchitecture.Domain.Metamodel.Hydration;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;

public interface ISaveAndHydrateRepository<T, H>
    where T : AggregateRoot, IAggregateRootWithHydration<H>
{
    Task SaveAndHydrate(T aggregate);
}
