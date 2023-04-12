using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

internal class PeriodicOutboxAwaker : PausableBackgroundService
{
    private readonly Duration _period;
    private readonly OutboxFlushRequestsChannel _requestsChannel;

    public PeriodicOutboxAwaker(Duration period, OutboxFlushRequestsChannel requestsChannel)
    {
        _period = period;
        _requestsChannel = requestsChannel;
    }

    protected override async Task ExecuteUntilPausedAsync(CancellationToken pausingToken)
    {
        while (!pausingToken.IsCancellationRequested)
        {
            try
            {
                _requestsChannel.RequestNewFlush();
                await Task.Delay(_period.ToTimeSpan(), pausingToken);
            }
            catch (OperationCanceledException) when (pausingToken.IsCancellationRequested)
            {
            }
        }
    }
}
