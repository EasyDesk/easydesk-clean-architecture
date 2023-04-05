using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

internal class OutboxFlusherBackgroundService : BackgroundConsumer<Nothing>
{
    private readonly OutboxFlushRequestsChannel _requestsChannel;
    private readonly ILogger<OutboxFlusherBackgroundService> _logger;

    public OutboxFlusherBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        OutboxFlushRequestsChannel requestsChannel,
        ILogger<OutboxFlusherBackgroundService> logger) : base(serviceScopeFactory)
    {
        _requestsChannel = requestsChannel;
        _logger = logger;
    }

    protected override IAsyncEnumerable<Nothing> GetProducer(CancellationToken pausingToken) =>
        _requestsChannel.GetAllFlushRequests(pausingToken);

    protected override async Task Consume(Nothing item, IServiceProvider serviceProvider, CancellationToken pausingToken)
    {
        await serviceProvider.GetRequiredService<OutboxFlusher>().Flush();
        _logger.LogDebug("Correctly flushed outbox");
    }

    protected override void OnException(Nothing item, IServiceProvider serviceProvider, Exception exception)
    {
        _logger.LogError(exception, "Unexpected error while publishing outbox messages.");
    }
}
