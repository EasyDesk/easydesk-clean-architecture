using EasyDesk.CleanArchitecture.Application.Messaging.Messages;
using Rebus.Handlers;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public class EventHandlerAdapter<M> : IHandleMessages<M>
    where M : IIncomingMessage
{
    private readonly IMessageHandler<M> _innerHandler;

    public EventHandlerAdapter(IMessageHandler<M> innerHandler)
    {
        _innerHandler = innerHandler;
    }

    public async Task Handle(M message)
    {
        await _innerHandler.Handle(message).ThenThrowIfFailure();
    }
}
