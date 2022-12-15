namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;

public interface IRemoveByIdRepository<T, TId> : IGetByIdRepository<T, TId>, IRemoveRepository<T>
    where T : AggregateRoot
{
    public async Task<Option<T>> RemoveById(TId id) => await GetById(id).ThenIfPresent(Remove);
}
