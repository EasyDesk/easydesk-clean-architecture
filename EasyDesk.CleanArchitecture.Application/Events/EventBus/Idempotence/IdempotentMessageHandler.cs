using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.EventBus.Idempotence
{
    public class IdempotentMessageHandler : IEventBusMessageHandler
    {
        private readonly IEventBusMessageHandler _handler;
        private readonly IIdempotenceManager _idempotenceManager;

        public IdempotentMessageHandler(IEventBusMessageHandler handler, IIdempotenceManager idempotenceManager)
        {
            _handler = handler;
            _idempotenceManager = idempotenceManager;
        }
        
        public async Task<EventBusMessageHandlerResult> Handle(EventBusMessage message)
        {
            if (await _idempotenceManager.HasBeenProcessed(message))
            {
                return EventBusMessageHandlerResult.Handled;
            }
            var result = await _handler.Handle(message);
            await _idempotenceManager.MarkAsProcessed(message);
            return result;
        }
    }
}
