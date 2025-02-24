using Autofac;
using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using EasyDesk.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace EasyDesk.CleanArchitecture.Infrastructure.Auditing;

public delegate Task AuditingExceptionHandler(AuditRecord record, Exception exception);

internal class AuditingBackgroundTask : BackgroundConsumer<(AuditRecord, TenantInfo)>
{
    private readonly ChannelReader<(AuditRecord, TenantInfo)> _auditsChannel;
    private readonly ILogger<AuditingBackgroundTask> _logger;

    public AuditingBackgroundTask(
        ILifetimeScope lifetimeScope,
        ChannelReader<(AuditRecord, TenantInfo)> auditsChannel,
        ILogger<AuditingBackgroundTask> logger) : base(lifetimeScope)
    {
        _auditsChannel = auditsChannel;
        _logger = logger;
    }

    protected override IAsyncEnumerable<(AuditRecord, TenantInfo)> GetProducer(CancellationToken pausingToken) =>
        _auditsChannel.ReadAllAsync(pausingToken);

    protected override async Task Consume(
        (AuditRecord, TenantInfo) item,
        ILifetimeScope lifetimeScope,
        CancellationToken pausingToken)
    {
        var (record, tenantInfo) = item;
        lifetimeScope.ResolveOption<IContextTenantInitializer>().IfPresent(i => i.Initialize(tenantInfo));
        await lifetimeScope.Resolve<IAuditStorageImplementation>().StoreAudit(record);
        _logger.LogDebug("Stored audit with name {auditName}", record.Name);
    }

    protected override async Task OnException(
        (AuditRecord, TenantInfo) item,
        ILifetimeScope lifetimeScope,
        Exception exception,
        CancellationToken pausingToken)
    {
        var (record, _) = item;
        _logger.LogError(exception, "Unexpected error while registering audit with name {auditName}.", record.Name);
        await lifetimeScope
            .ResolveOption<AuditingExceptionHandler>()
            .IfPresentAsync(h => h(record, exception));
    }
}
