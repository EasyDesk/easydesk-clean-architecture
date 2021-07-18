using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.Tools.Observables;
using EasyDesk.Tools.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Events.EventBus.Outbox
{
    public class OutboxPublisher : IEventBusPublisher
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOutbox _outbox;
        private readonly IOutboxChannel _outboxChannel;

        public OutboxPublisher(IUnitOfWork unitOfWork, IOutbox outbox, IOutboxChannel outboxChannel)
        {
            _unitOfWork = unitOfWork;
            _outbox = outbox;
            _outboxChannel = outboxChannel;
        }

        public async Task Publish(IEnumerable<EventBusMessage> messages)
        {
            await _outbox.StoreMessages(messages);
            _unitOfWork.AfterCommit.Subscribe(context => TryPublishingMessages(context, messages));
        }

        private void TryPublishingMessages(AfterCommitContext context, IEnumerable<EventBusMessage> messages)
        {
            if (!context.Successful)
            {
                return;
            }
            var messageIds = messages.Select(m => m.Id);
            _outboxChannel.OnNewMessageGroup(messageIds);
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
