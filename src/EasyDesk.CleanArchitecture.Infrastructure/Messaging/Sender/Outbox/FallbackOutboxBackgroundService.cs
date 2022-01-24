using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Sender.Outbox;

public class FallbackOutboxBackgroundService : BackgroundService
{
    private static readonly TimeSpan _pollingPeriod = TimeSpan.FromSeconds(30);
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<FallbackOutboxBackgroundService> _logger;

    public FallbackOutboxBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<FallbackOutboxBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var outbox = scope.ServiceProvider.GetRequiredService<IOutbox>();
                    await outbox.Flush();
                }
                await Task.Delay(_pollingPeriod, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending outbox messages from Fallback sender");
            }
        }
    }
}
