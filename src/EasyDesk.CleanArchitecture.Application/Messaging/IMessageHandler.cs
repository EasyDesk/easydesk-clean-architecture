namespace EasyDesk.CleanArchitecture.Application.Messaging;

public interface IMessageHandler<M> where M : IMessage
{
    Task<Result<Nothing>> Handle(M message);
}
