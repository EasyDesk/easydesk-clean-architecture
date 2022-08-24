namespace EasyDesk.CleanArchitecture.Application.Messaging;

public interface IMessagePublisher
{
    Task Publish(IMessage message, Action<MessageOptions> configure = null);
}
