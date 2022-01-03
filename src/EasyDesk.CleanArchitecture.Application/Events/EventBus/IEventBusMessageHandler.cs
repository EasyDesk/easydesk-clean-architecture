using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.EventBus;

public enum EventBusMessageHandlerResult
{
    Handled,
    TransientFailure,
    GenericFailure,
    NotSupported
}

public interface IEventBusMessageHandler
{
    Task<EventBusMessageHandlerResult> Handle(EventBusMessage message);
}
