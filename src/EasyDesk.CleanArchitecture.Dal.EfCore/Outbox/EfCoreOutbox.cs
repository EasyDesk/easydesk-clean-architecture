using EasyDesk.CleanArchitecture.Application.Messaging.MessageBroker;
using EasyDesk.CleanArchitecture.Application.Messaging.MessageBroker.Outbox;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools.Options;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Outbox;

public class EfCoreOutbox : IOutbox
{
    public static readonly Duration MessageAgingTime = Duration.FromSeconds(30);

    private readonly OutboxContext _outboxContext;
    private readonly EfCoreTransactionManager _unitOfWork;
    private readonly IMessagePublisher _publisher;
    private readonly ITimestampProvider _timestampProvider;

    public EfCoreOutbox(
        OutboxContext outboxContext,
        EfCoreTransactionManager unitOfWork,
        IMessagePublisher publisher,
        ITimestampProvider timestampProvider)
    {
        _outboxContext = outboxContext;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
        _timestampProvider = timestampProvider;
    }

    public async Task StoreMessages(IEnumerable<Message> messages)
    {
        await _unitOfWork.RegisterExternalDbContext(_outboxContext);
        var outboxMessages = messages.Select(ToOutboxMessage);
        _outboxContext.Messages.AddRange(outboxMessages);
        await _outboxContext.SaveChangesAsync();
    }

    private static OutboxMessage ToOutboxMessage(Message message)
    {
        return new OutboxMessage
        {
            Id = message.Id,
            Content = message.Content,
            Type = message.Type,
            Timestamp = message.Timestamp
        };
    }

    public async Task PublishMessages(IEnumerable<Guid> messageIds)
    {
        await Publish(q => q.Where(m => messageIds.Contains(m.Id)));
    }

    public async Task Flush()
    {
        var from = _timestampProvider.Now - MessageAgingTime.AsTimeOffset;
        await Publish(q => q.Where(m => m.Timestamp < from));
    }

    private async Task Publish(QueryWrapper<OutboxMessage> filter)
    {
        var outboxMessages = await _outboxContext.Messages
            .Wrap(filter)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        if (!outboxMessages.Any())
        {
            return;
        }

        var messages = outboxMessages.Select(m => new Message(
            m.Id, m.Timestamp, m.Type, m.TenantId.AsOption(), m.Content));

        await _publisher.Publish(messages);

        _outboxContext.Messages.RemoveRange(outboxMessages);
        await _outboxContext.SaveChangesAsync();
    }
}
