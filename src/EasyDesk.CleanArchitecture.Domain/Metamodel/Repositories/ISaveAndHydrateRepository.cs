using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;

public interface ISaveAndHydrateRepository<T>
    where T : AggregateRoot
{
    Task SaveAndHydrate(T aggregate);
}
