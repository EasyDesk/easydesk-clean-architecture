using EasyDesk.CleanArchitecture.Application.Messaging.Messages;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public interface IMessageHandler<M> where M : IIncomingMessage
{
    Task<Result<Nothing>> Handle(M message);
}
