namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;

public interface ISaveRepository<T>
    where T : AggregateRoot
{
    Task Save(T aggregate);
}
