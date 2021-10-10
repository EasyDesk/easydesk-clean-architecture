using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Events.ExternalEvents;

namespace EasyDesk.CleanArchitecture.Application.Mediator
{
    public abstract class ExternalEventHandlerBase<T> : NotificationHandlerBase<T>
        where T : IExternalEvent
    {
        protected ExternalEventHandlerBase()
        {
        }

        protected ExternalEventHandlerBase(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
