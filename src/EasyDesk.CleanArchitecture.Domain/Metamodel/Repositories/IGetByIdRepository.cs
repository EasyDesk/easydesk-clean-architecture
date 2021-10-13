using EasyDesk.CleanArchitecture.Domain.Metamodel.Results;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories
{
    public interface IGetByIdRepository<T, TId>
        where T : AggregateRoot
    {
        Task<Result<T>> GetById(TId id);
    }
}
