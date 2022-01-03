using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.Mediator;

public abstract class DomainEventHandlerBase<T> : NotificationHandlerBase<T>
    where T : DomainEvent
{
    protected DomainEventHandlerBase()
    {
    }

    protected DomainEventHandlerBase(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}
