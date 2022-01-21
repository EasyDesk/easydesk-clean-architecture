using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Receiver.Idempotence;

public class IdempotentMessageHandler : IMessageHandler
{
    private readonly IMessageHandler _handler;
    private readonly IIdempotenceManager _idempotenceManager;

    public IdempotentMessageHandler(IMessageHandler handler, IIdempotenceManager idempotenceManager)
    {
        _handler = handler;
        _idempotenceManager = idempotenceManager;
    }

    public async Task<MessageHandlerResult> Handle(Message message)
    {
        if (await _idempotenceManager.HasBeenProcessed(message))
        {
            return MessageHandlerResult.Handled;
        }
        var result = await _handler.Handle(message);
        await _idempotenceManager.MarkAsProcessed(message);
        return result;
    }
}
