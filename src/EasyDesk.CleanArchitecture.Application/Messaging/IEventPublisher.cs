using EasyDesk.CleanArchitecture.Application.Cqrs.Events;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public interface IEventPublisher
{
    Task Publish<T>(T message) where T : IOutgoingEvent, IMessage;
}
