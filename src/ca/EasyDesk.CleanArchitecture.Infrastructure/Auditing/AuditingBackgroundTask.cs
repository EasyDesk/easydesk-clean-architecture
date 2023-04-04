using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace EasyDesk.CleanArchitecture.Infrastructure.Auditing;

internal class AuditingBackgroundTask : PausableBackgroundService
{
    private readonly ChannelReader<(AuditRecord, TenantInfo)> _auditsChannel;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<AuditingBackgroundTask> _logger;

    public AuditingBackgroundTask(
        ChannelReader<(AuditRecord, TenantInfo)> auditsChannel,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<AuditingBackgroundTask> logger)
    {
        _auditsChannel = auditsChannel;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteUntilPausedAsync(CancellationToken pausingToken)
    {
        await foreach (var (record, tenantInfo) in _auditsChannel.ReadAllAsync(pausingToken))
        {
            try
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                scope.ServiceProvider.GetRequiredService<IContextTenantInitializer>().Initialize(tenantInfo);
                await scope.ServiceProvider.GetRequiredService<IAuditStorageImplementation>().StoreAudit(record);
                _logger.LogDebug("Stored audit with name {auditName}", record.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while registering audit with name {auditName}.", record.Name);
            }
        }
    }
}
