using EasyDesk.CleanArchitecture.Application.Cqrs.Operations;
using EasyDesk.CleanArchitecture.Application.Dispatching;

namespace EasyDesk.CleanArchitecture.Application.Cqrs.Events;

public interface IIncomingEvent : IEvent, IReadWriteOperation, IDispatchable<Nothing>
{
}
