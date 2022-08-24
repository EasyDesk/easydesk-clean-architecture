using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Outbox;

public class OutboxFlusherBackgroundService : BackgroundService
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in _requestsChannel.GetAllFlushRequests())
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                await scope.ServiceProvider.GetRequiredService<OutboxFlusher>().Flush();
                _logger.LogInformation("Correctly flushed outbox");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while publishing outbox messages.");
            }
        }
    }
}
