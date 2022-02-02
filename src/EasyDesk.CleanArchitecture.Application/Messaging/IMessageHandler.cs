using Rebus.Handlers;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public interface IMessageHandler<M> : IHandleMessages<M>
    where M : IMessage
{
}
