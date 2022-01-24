using EasyDesk.CleanArchitecture.Application.Messaging.Sender;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Sender.Outbox;
using EasyDesk.Tools.Options;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Outbox;

public class EfCoreOutbox : IOutbox
{
    public static readonly Duration MessageAgingTime = Duration.FromSeconds(30);

    private readonly OutboxContext _outboxContext;
    private readonly EfCoreTransactionManager _transactionManager;
    private readonly IMessageSender _publisher;
    private readonly ITimestampProvider _timestampProvider;
    private readonly OutboxSerializer _outboxSerializer;

    public EfCoreOutbox(
        OutboxContext outboxContext,
        EfCoreTransactionManager transactionManager,
        IMessageSender publisher,
        ITimestampProvider timestampProvider,
        OutboxSerializer outboxSerializer)
    {
        _outboxContext = outboxContext;
        _transactionManager = transactionManager;
        _publisher = publisher;
        _timestampProvider = timestampProvider;
        _outboxSerializer = outboxSerializer;
    }

    public async Task StoreMessages(IEnumerable<Message> messages)
    {
        await _transactionManager.RegisterExternalDbContext(_outboxContext);
        var outboxMessages = messages.Select(m => ToOutboxMessage(m, _timestampProvider.Now));
        _outboxContext.Messages.AddRange(outboxMessages);
        await _outboxContext.SaveChangesAsync();
    }

    private OutboxMessage ToOutboxMessage(Message message, Timestamp timestamp)
    {
        return new OutboxMessage
        {
            Id = message.Id,
            Content = _outboxSerializer.Serialize(message.Content),
            Metadata = _outboxSerializer.Serialize(message.Metadata),
            EnqueuedTimestamp = timestamp,
            Type = message.Content.GetType().AssemblyQualifiedName
        };
    }

    public async Task PublishMessages(IEnumerable<Guid> messageIds)
    {
        await Publish(q => q.Where(m => messageIds.Contains(m.Id)));
    }

    public async Task Flush()
    {
        var from = _timestampProvider.Now - MessageAgingTime.AsTimeOffset;
        await Publish(q => q.Where(m => m.EnqueuedTimestamp < from));
    }

    private async Task Publish(QueryWrapper<OutboxMessage> filter)
    {
        var outboxMessages = await _outboxContext.Messages
            .Wrap(filter)
            .OrderBy(m => m.EnqueuedTimestamp)
            .ToListAsync();

        if (!outboxMessages.Any())
        {
            return;
        }

        var messages = outboxMessages.Select(ToMessage);

        await _publisher.Send(messages);

        _outboxContext.Messages.RemoveRange(outboxMessages);
        await _outboxContext.SaveChangesAsync();
    }

    private Message ToMessage(OutboxMessage outboxMessage)
    {
        var messageType = Type.GetType(outboxMessage.Type);
        return new Message(
            Id: outboxMessage.Id,
            Content: _outboxSerializer.Deserialize(outboxMessage.Content, messageType),
            Metadata: _outboxSerializer.Deserialize<IImmutableDictionary<string, string>>(outboxMessage.Metadata));
    }
}
