using EasyDesk.CleanArchitecture.Domain.Metamodel.Results;
using EasyDesk.Tools;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel;

public interface IDomainEventHandler<T>
    where T : DomainEvent
{
    Task<Result<Nothing>> Handle(T ev);
}
