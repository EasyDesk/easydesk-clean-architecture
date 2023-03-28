namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;

public interface IRemoveRepository<T>
    where T : AggregateRoot
{
    void Remove(T aggregate);
}
