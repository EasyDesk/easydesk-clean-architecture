using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using EasyDesk.CleanArchitecture.Application.Events.EventBus.Outbox;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Outbox
{
    public class EfCoreOutbox : IOutbox
    {
        public static readonly Duration MessageAgingTime = Duration.FromSeconds(30);

        private readonly OutboxContext _outboxContext;
        private readonly EfCoreTransactionManager _unitOfWork;
        private readonly IEventBusPublisher _eventBus;
        private readonly ITimestampProvider _timestampProvider;

        public EfCoreOutbox(
            OutboxContext outboxContext,
            EfCoreTransactionManager unitOfWork,
            IEventBusPublisher eventBus,
            ITimestampProvider timestampProvider)
        {
            _outboxContext = outboxContext;
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _timestampProvider = timestampProvider;
        }

        public async Task StoreMessages(IEnumerable<EventBusMessage> messages)
        {
            await _unitOfWork.RegisterExternalDbContext(_outboxContext);
            var outboxMessages = messages.Select(ToOutboxMessage);
            _outboxContext.Messages.AddRange(outboxMessages);
            await _outboxContext.SaveChangesAsync();
        }

        private static OutboxMessage ToOutboxMessage(EventBusMessage message)
        {
            return new OutboxMessage
            {
                Id = message.Id,
                Content = message.Content,
                EventType = message.EventType,
                OccurredAt = message.OccurredAt
            };
        }

        public async Task PublishMessages(IEnumerable<Guid> messageIds)
        {
            await Publish(q => q.Where(m => messageIds.Contains(m.Id)));
        }

        public async Task Flush()
        {
            var from = _timestampProvider.Now - MessageAgingTime.AsTimeOffset;
            await Publish(q => q.Where(m => m.OccurredAt < from));
        }

        private async Task Publish(QueryWrapper<OutboxMessage> filter)
        {
            var outboxMessages = await _outboxContext.Messages
                .Wrap(filter)
                .OrderBy(m => m.OccurredAt)
                .ToListAsync();

            if (!outboxMessages.Any())
            {
                return;
            }

            var eventBusMessages = outboxMessages.Select(m => new EventBusMessage(m.Id, m.EventType, m.Content, m.OccurredAt));

            await _eventBus.Publish(eventBusMessages);

            _outboxContext.Messages.RemoveRange(outboxMessages);
            await _outboxContext.SaveChangesAsync();
        }
    }
}
