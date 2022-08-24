namespace EasyDesk.CleanArchitecture.Application.Messaging;

public interface IMessageSender
{
    Task Send(IMessage message, Action<MessageOptions> configure = null);

    Task SendLocal(IMessage message, Action<MessageOptions> configure = null);
}
