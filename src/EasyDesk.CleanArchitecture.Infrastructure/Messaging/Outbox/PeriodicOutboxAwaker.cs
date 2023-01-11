using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

internal class PeriodicOutboxAwaker : PausableBackgroundService
{
    private readonly Duration _period;
    private readonly OutboxFlushRequestsChannel _requestsChannel;
    private readonly ILogger<PeriodicOutboxAwaker> _logger;

    public PeriodicOutboxAwaker(Duration period, OutboxFlushRequestsChannel requestsChannel, ILogger<PeriodicOutboxAwaker> logger)
    {
        _period = period;
        _requestsChannel = requestsChannel;
        _logger = logger;
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while requesting a new outbox flush");
            }
        }
    }
}
