namespace EasyDesk.CleanArchitecture.Application.Messaging;

public interface IEventPublisher
{
    Task Publish(IOutgoingEvent message, Action<MessageOptions> configure = null);
}
