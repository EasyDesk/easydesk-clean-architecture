using EasyDesk.CleanArchitecture.Application.Data;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.EventBus
{
    public class TransactionalEventBusMessageHandler : IEventBusMessageHandler
    {
        private readonly IEventBusMessageHandler _handler;
        private readonly ITransactionManager _transactionManager;

        public TransactionalEventBusMessageHandler(IEventBusMessageHandler handler, ITransactionManager transactionManager)
        {
            _handler = handler;
            _transactionManager = transactionManager;
        }

        public async Task<EventBusMessageHandlerResult> Handle(EventBusMessage message)
        {
            await _transactionManager.Begin();
            var handlerResult = await _handler.Handle(message);
            var commitResult = await _transactionManager.Commit();
            return commitResult.Match(
                success: _ => handlerResult,
                failure: _ => EventBusMessageHandlerResult.TransientFailure);
        }
    }
}
