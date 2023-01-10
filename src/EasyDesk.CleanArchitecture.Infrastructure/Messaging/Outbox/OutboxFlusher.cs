using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.Tools.Collections;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

internal class OutboxFlusher
{
    private readonly int _batchSize;
    private readonly IUnitOfWorkProvider _unitOfWorkProvider;
    private readonly IOutbox _outbox;

    public OutboxFlusher(
        RebusMessagingOptions options,
        IUnitOfWorkProvider unitOfWorkProvider,
        IOutbox outbox)
    {
        _batchSize = options.OutboxOptions.FlushingBatchSize;
        _unitOfWorkProvider = unitOfWorkProvider;
        _outbox = outbox;
    }

    public async Task Flush(ITransport transport)
    {
        await _unitOfWorkProvider.RunTransactionally(() => FlushWithinTransaction(transport));
    }

    private async Task FlushWithinTransaction(ITransport transport)
    {
        using var scope = new RebusTransactionScope();
        while (await SendNextBatch(transport, scope.TransactionContext))
        {
        }
        await scope.CompleteAsync();
    }

    private async Task<bool> SendNextBatch(ITransport transport, ITransactionContext transactionContext)
    {
        var messages = await _outbox.RetrieveNextMessages(_batchSize);
        if (messages.IsEmpty())
        {
            return false;
        }
        foreach (var (message, destination) in messages)
        {
            await transport.Send(destination, message, transactionContext);
        }
        return messages.Count() == _batchSize;
    }
}
