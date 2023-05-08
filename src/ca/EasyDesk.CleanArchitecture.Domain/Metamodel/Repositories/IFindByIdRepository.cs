namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;

public interface IFindByIdRepository<T, TId>
    where T : AggregateRoot
{
    IAggregateView<T> FindById(TId id);
}
