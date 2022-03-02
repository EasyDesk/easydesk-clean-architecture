using EasyDesk.Tools.Results;
using Rebus.Handlers;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public class MessageHandlerAdapter<M> : IHandleMessages<M>
    where M : IMessage
{
    private readonly IMessageHandler<M> _innerHandler;

    public MessageHandlerAdapter(IMessageHandler<M> innerHandler)
    {
        _innerHandler = innerHandler;
    }

    public async Task Handle(M message)
    {
        await _innerHandler.Handle(message).ThenThrowIfFailure();
    }
}
