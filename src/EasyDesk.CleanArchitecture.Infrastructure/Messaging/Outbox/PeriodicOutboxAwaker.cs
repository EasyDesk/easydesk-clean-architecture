using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

internal class PeriodicOutboxAwaker : PausableBackgroundService
{
    private readonly Duration _period;
    private readonly OutboxFlushRequestsChannel _requestsChannel;
    private readonly ILogger<PeriodicOutboxAwaker> _logger;
    private readonly RebusTransportProvider _transportProvider;

    public PeriodicOutboxAwaker(
        RebusMessagingOptions options,
        OutboxFlushRequestsChannel requestsChannel,
        ILogger<PeriodicOutboxAwaker> logger,
        RebusTransportProvider transportProvider)
    {
        _period = options.OutboxOptions.FlushingPeriod;
        _requestsChannel = requestsChannel;
        _logger = logger;
        _transportProvider = transportProvider;
    }

    protected override async Task ExecuteUntilPausedAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _requestsChannel.RequestNewFlush(_transportProvider());
                await Task.Delay(_period.ToTimeSpan(), stoppingToken);
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while requesting a new outbox flush");
            }
        }
    }
}
