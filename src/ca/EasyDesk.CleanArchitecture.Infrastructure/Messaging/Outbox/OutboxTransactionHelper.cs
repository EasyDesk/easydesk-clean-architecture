namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

public class OutboxTransactionHelper
{
    private readonly OutboxFlushRequestsChannel _requestsChannel;
    private bool _flushRequestWasRegistered;

    public OutboxTransactionHelper(OutboxFlushRequestsChannel requestsChannel)
    {
        _requestsChannel = requestsChannel;
    }

    public void NotifyNewOutgoingMessage()
    {
        _flushRequestWasRegistered = true;
    }

    public void RequestNewFlushIfNecessary()
    {
        if (!_flushRequestWasRegistered)
        {
            return;
        }

        _requestsChannel.RequestNewFlush();
    }
}
