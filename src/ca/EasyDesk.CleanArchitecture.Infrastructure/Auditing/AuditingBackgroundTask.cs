using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace EasyDesk.CleanArchitecture.Infrastructure.Auditing;

internal class AuditingBackgroundTask : BackgroundConsumer<(AuditRecord, TenantInfo)>
{
    private readonly ChannelReader<(AuditRecord, TenantInfo)> _auditsChannel;
    private readonly ILogger<AuditingBackgroundTask> _logger;

    public AuditingBackgroundTask(
        IServiceScopeFactory serviceScopeFactory,
        ChannelReader<(AuditRecord, TenantInfo)> auditsChannel,
        ILogger<AuditingBackgroundTask> logger) : base(serviceScopeFactory)
    {
        _auditsChannel = auditsChannel;
        _logger = logger;
    }

    protected override IAsyncEnumerable<(AuditRecord, TenantInfo)> GetProducer(CancellationToken pausingToken) =>
        _auditsChannel.ReadAllAsync(pausingToken);

    protected override async Task Consume(
        (AuditRecord, TenantInfo) item,
        IServiceProvider serviceProvider,
        CancellationToken pausingToken)
    {
        var (record, tenantInfo) = item;
        serviceProvider.GetRequiredService<IContextTenantInitializer>().Initialize(tenantInfo);
        await serviceProvider.GetRequiredService<IAuditStorageImplementation>().StoreAudit(record);
        _logger.LogDebug("Stored audit with name {auditName}", record.Name);
    }

    protected override void OnException(
        (AuditRecord, TenantInfo) item,
        IServiceProvider serviceProvider,
        Exception exception)
    {
        var (record, _) = item;
        _logger.LogError(exception, "Unexpected error while registering audit with name {auditName}.", record.Name);
    }
}
