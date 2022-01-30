using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Outbox;

public class PeriodicOutboxAwaker : BackgroundService
{
    private readonly Duration _period;
    private readonly OutboxFlushRequestsChannel _requestsChannel;

    public PeriodicOutboxAwaker(Duration period, OutboxFlushRequestsChannel requestsChannel)
    {
        _period = period;
        _requestsChannel = requestsChannel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _requestsChannel.RequestNewFlush();
            await Task.Delay(_period.AsTimeSpan);
        }
    }
}
