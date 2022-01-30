using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.Tools.Collections;
using Rebus.Transport;
using System.Linq;
using System.Threading.Tasks;
using static EasyDesk.Tools.Functions;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Outbox;

public class OutboxFlusher
{
    private readonly int _batchSize;
    private readonly ITransactionManager _transactionManager;
    private readonly IOutbox _outbox;
    private readonly ITransport _transport;

    public OutboxFlusher(int batchSize, ITransactionManager transactionManager, IOutbox outbox, ITransport transport)
    {
        _batchSize = batchSize;
        _transactionManager = transactionManager;
        _outbox = outbox;
        _transport = transport;
    }

    public async Task Flush()
    {
        await _transactionManager.RunTransactionally(() => Execute(FlushWithinTransaction));
    }

    private async Task FlushWithinTransaction()
    {
        using var scope = new RebusTransactionScope();
        while (await SendNextBatch(scope.TransactionContext))
        {
        }
        await scope.CompleteAsync();
    }

    private async Task<bool> SendNextBatch(ITransactionContext transactionContext)
    {
        var messages = await _outbox.RetrieveNextMessages(_batchSize);
        if (messages.IsEmpty())
        {
            return false;
        }
        foreach (var (message, destination) in messages)
        {
            await _transport.Send(destination, message, transactionContext);
        }
        return messages.Count() == _batchSize;
    }
}
