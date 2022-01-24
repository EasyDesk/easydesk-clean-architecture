using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Messaging.Sender;
using EasyDesk.Tools.Observables;
using EasyDesk.Tools.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Sender.Outbox;

public sealed class OutboxSender : IMessageSender
{
    private readonly ITransactionManager _transactionManager;
    private readonly IOutbox _outbox;
    private readonly IOutboxChannel _outboxChannel;

    public OutboxSender(ITransactionManager transactionManager, IOutbox outbox, IOutboxChannel outboxChannel)
    {
        _transactionManager = transactionManager;
        _outbox = outbox;
        _outboxChannel = outboxChannel;
    }

    public async Task Send(IEnumerable<Message> messages)
    {
        await _outbox.StoreMessages(messages);
        _transactionManager.AfterCommit.Subscribe(context => TrySendingMessages(context, messages));
    }

    private void TrySendingMessages(AfterCommitContext context, IEnumerable<Message> messages)
    {
        if (!context.Successful)
        {
            return;
        }
        var messageIds = messages.Select(m => m.Id);
        _outboxChannel.OnNewMessageGroup(messageIds);
    }

    public void Dispose()
    {
    }
}
