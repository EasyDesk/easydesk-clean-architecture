namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;

public interface ISaveRepository<T>
    where T : AggregateRoot
{
    void Save(T entity);
}
