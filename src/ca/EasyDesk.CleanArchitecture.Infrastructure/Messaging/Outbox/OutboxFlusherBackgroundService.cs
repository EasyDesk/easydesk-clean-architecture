using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

internal class OutboxFlusherBackgroundService : PausableBackgroundService
{
    private readonly OutboxFlushRequestsChannel _requestsChannel;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OutboxFlusherBackgroundService> _logger;

    public OutboxFlusherBackgroundService(
        OutboxFlushRequestsChannel requestsChannel,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OutboxFlusherBackgroundService> logger)
    {
        _requestsChannel = requestsChannel;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteUntilPausedAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in _requestsChannel.GetAllFlushRequests(stoppingToken))
        {
            try
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<OutboxFlusher>().Flush();
                _logger.LogDebug("Correctly flushed outbox");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while publishing outbox messages.");
            }
        }
    }
}
