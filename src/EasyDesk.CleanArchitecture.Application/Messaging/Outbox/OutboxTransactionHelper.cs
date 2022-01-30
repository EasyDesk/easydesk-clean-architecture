using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.Tools.Observables;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Outbox;

public class OutboxTransactionHelper
{
    private readonly IOutbox _outbox;
    private readonly ITransactionManager _transactionManager;
    private readonly OutboxFlushRequestsChannel _requestsChannel;
    private bool _flushRequestWasRegistered = false;

    public OutboxTransactionHelper(IOutbox outbox, ITransactionManager transactionManager, OutboxFlushRequestsChannel requestsChannel)
    {
        _outbox = outbox;
        _transactionManager = transactionManager;
        _requestsChannel = requestsChannel;
    }

    public void EnsureCommitHooksAreRegistered()
    {
        if (_flushRequestWasRegistered)
        {
            return;
        }

        _transactionManager.BeforeCommit.Subscribe(_ => _outbox.StoreEnqueuedMessages());
        _transactionManager.AfterCommit.Subscribe(context => _requestsChannel.RequestNewFlush());
        _flushRequestWasRegistered = true;
    }
}
