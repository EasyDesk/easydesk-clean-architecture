using EasyDesk.Tools.Results;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;

public interface IRemoveByIdRepository<T, TId> : IGetByIdRepository<T, TId>, IRemoveRepository<T>
    where T : AggregateRoot
{
    public async Task<Result<T>> RemoveById(TId id) => await GetById(id).ThenIfSuccess(Remove);
}
