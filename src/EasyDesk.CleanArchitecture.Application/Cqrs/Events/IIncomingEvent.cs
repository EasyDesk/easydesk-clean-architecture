using EasyDesk.CleanArchitecture.Application.Dispatching;

namespace EasyDesk.CleanArchitecture.Application.Cqrs.Events;

public interface IIncomingEvent : IEvent, IDispatchable<Nothing>
{
}
