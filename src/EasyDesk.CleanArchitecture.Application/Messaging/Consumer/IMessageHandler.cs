using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.MessageBroker;

public enum MessageHandlerResult
{
    Handled,
    TransientFailure,
    GenericFailure,
    NotSupported
}

public interface IMessageHandler
{
    Task<MessageHandlerResult> Handle(Message message);
}
