namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;

public interface ISaveAndHydrateRepository<T>
    where T : AggregateRoot
{
    Task<T> SaveAndHydrate(T aggregate);
}
