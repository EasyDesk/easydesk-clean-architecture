using EasyDesk.Tools.Options;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories
{
    public interface IGetByIdRepository<T, TId>
        where T : AggregateRoot
    {
        Task<Option<T>> GetById(TId id);
    }
}
