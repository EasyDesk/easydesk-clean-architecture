using Rebus.Handlers;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Receiver;

public interface IMessageHandler<M> : IHandleMessages<M>
    where M : IMessage
{
}
