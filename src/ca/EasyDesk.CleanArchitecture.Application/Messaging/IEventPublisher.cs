using EasyDesk.CleanArchitecture.Application.Cqrs.Async;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public interface IEventPublisher
{
    Task Publish<T>(T message) where T : IOutgoingEvent;
}
