using EasyDesk.CleanArchitecture.Application.Data;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.EventBus
{
    public class TransactionalEventBusMessageHandler : IEventBusMessageHandler
    {
        private readonly IEventBusMessageHandler _handler;
        private readonly IUnitOfWork _unitOfWork;

        public TransactionalEventBusMessageHandler(IEventBusMessageHandler handler, IUnitOfWork unitOfWork)
        {
            _handler = handler;
            _unitOfWork = unitOfWork;
        }

        public async Task<EventBusMessageHandlerResult> Handle(EventBusMessage message)
        {
            await _unitOfWork.Begin();
            var handlerResult = await _handler.Handle(message);
            var commitResult = await _unitOfWork.Commit();
            return commitResult.Match(
                success: _ => handlerResult,
                failure: _ => EventBusMessageHandlerResult.TransientFailure);
        }
    }
}
