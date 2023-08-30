using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.Commons.Options;
using Rebus.Transport;
using System.Diagnostics;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

internal class OutboxFlusher
{
    private readonly Func<int, bool> _batchPredicate;
    private readonly Option<int> _batchSize;
    private readonly IUnitOfWorkProvider _unitOfWorkProvider;
    private readonly IOutbox _outbox;
    private readonly ITransport _transport;

    public OutboxFlusher(
        OutboxFlushingStrategy flushingStrategy,
        IUnitOfWorkProvider unitOfWorkProvider,
        IOutbox outbox,
        ITransport transport)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
        _outbox = outbox;
        _transport = transport;
        _batchPredicate = flushingStrategy switch
        {
            OutboxFlushingStrategy.AllAtOnce
                or OutboxFlushingStrategy.AllInBatches => _ => true,
            OutboxFlushingStrategy.Batched(var batches, _) => i => i < batches,
            _ => throw new UnreachableException()
        };
        _batchSize = flushingStrategy switch
        {
            OutboxFlushingStrategy.AllAtOnce => None,
            OutboxFlushingStrategy.AllInBatches(var batchSize) => Some(batchSize),
            OutboxFlushingStrategy.Batched(_, var batchSize) => Some(batchSize),
            _ => throw new UnreachableException()
        };
    }

    public async Task Flush()
    {
        await _unitOfWorkProvider.RunTransactionally(FlushWithinTransaction);
    }

    private async Task FlushWithinTransaction()
    {
        using var scope = new RebusTransactionScope();
        for (var i = 0; _batchPredicate(i) && await SendNextBatch(scope.TransactionContext); i++)
        {
        }
        await scope.CompleteAsync();
    }

    private async Task<bool> SendNextBatch(ITransactionContext transactionContext)
    {
        var messages = await _outbox.RetrieveNextMessages(_batchSize);
        foreach (var (message, destination) in messages)
        {
            await _transport.Send(destination, message, transactionContext);
        }
        return _batchSize.Contains(messages.Count());
    }
}
