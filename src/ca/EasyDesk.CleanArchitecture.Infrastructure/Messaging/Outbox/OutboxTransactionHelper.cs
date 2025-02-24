namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

public class OutboxTransactionHelper
{
    private readonly IOutbox _outbox;
    private readonly OutboxFlushRequestsChannel _requestsChannel;
    private bool _flushRequestWasRegistered = false;

    public OutboxTransactionHelper(IOutbox outbox, OutboxFlushRequestsChannel requestsChannel)
    {
        _outbox = outbox;
        _requestsChannel = requestsChannel;
    }

    public void NotifyNewOutgoingMessage()
    {
        if (_flushRequestWasRegistered)
        {
            return;
        }
        _flushRequestWasRegistered = true;
    }

    public void RequestNewFlushIfNecessary()
    {
        if (_flushRequestWasRegistered)
        {
            _requestsChannel.RequestNewFlush();
        }
    }

    public void StoreEnqueuedMessagesIfNecessary()
    {
        if (_flushRequestWasRegistered)
        {
            _outbox.StoreEnqueuedMessages();
        }
    }
}
