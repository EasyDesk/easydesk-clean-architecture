using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel;

public interface IDomainEventHandler<T>
    where T : DomainEvent
{
    Task<Result<Nothing>> Handle(T ev);
}
