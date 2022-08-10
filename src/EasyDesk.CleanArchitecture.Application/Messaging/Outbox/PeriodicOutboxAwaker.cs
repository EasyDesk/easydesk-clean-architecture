using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Outbox;

public class PeriodicOutboxAwaker : BackgroundService
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _requestsChannel.RequestNewFlush();
                await Task.Delay(_period.ToTimeSpan());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while requesting a new outbox flush");
                throw;
            }
        }
    }
}
