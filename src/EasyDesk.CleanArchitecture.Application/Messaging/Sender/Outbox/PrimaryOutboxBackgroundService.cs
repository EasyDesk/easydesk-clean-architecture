using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Sender.Outbox;

public class PrimaryOutboxBackgroundService : BackgroundService
{
    private readonly IOutboxChannel _outboxChannel;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<PrimaryOutboxBackgroundService> _logger;

    public PrimaryOutboxBackgroundService(
        IOutboxChannel outboxChannel,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<PrimaryOutboxBackgroundService> logger)
    {
        _outboxChannel = outboxChannel;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var messageGroup in _outboxChannel.GetAllMessageGroups())
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var outbox = scope.ServiceProvider.GetRequiredService<IOutbox>();
                    await outbox.PublishMessages(messageGroup);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while publishing outbox messages from primary outbox service");
            }
        }
    }
}
