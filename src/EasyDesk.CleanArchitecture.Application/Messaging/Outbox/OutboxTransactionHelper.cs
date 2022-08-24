using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.Tools.Observables;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Outbox;

public class OutboxTransactionHelper
{
    private readonly IOutbox _outbox;
    private readonly IUnitOfWorkProvider _unitOfWorkProvider;
    private readonly OutboxFlushRequestsChannel _requestsChannel;
    private bool _flushRequestWasRegistered = false;

    public OutboxTransactionHelper(IOutbox outbox, IUnitOfWorkProvider unitOfWorkProvider, OutboxFlushRequestsChannel requestsChannel)
    {
        _outbox = outbox;
        _unitOfWorkProvider = unitOfWorkProvider;
        _requestsChannel = requestsChannel;
    }

    public void EnsureCommitHooksAreRegistered()
    {
        if (_flushRequestWasRegistered)
        {
            return;
        }

        var unitOfWork = _unitOfWorkProvider.CurrentUnitOfWork
            .OrElseThrow(() => new InvalidOperationException("Unit of work was not started when registering outbox commit hooks"));

        unitOfWork.BeforeCommit.Subscribe(_ => _outbox.StoreEnqueuedMessages());
        unitOfWork.AfterCommit.Subscribe(_ => _requestsChannel.RequestNewFlush());
        _flushRequestWasRegistered = true;
    }
}
