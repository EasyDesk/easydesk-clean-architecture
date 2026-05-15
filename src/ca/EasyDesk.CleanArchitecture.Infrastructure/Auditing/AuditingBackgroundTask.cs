using Autofac;
using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using EasyDesk.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace EasyDesk.CleanArchitecture.Infrastructure.Auditing;

public delegate Task AuditingExceptionHandler(AuditRecord record, Exception exception);

internal class AuditingBackgroundTask : BackgroundConsumer<AuditRecord>
{
    private readonly ChannelReader<AuditRecord> _auditsChannel;
    private readonly ILogger<AuditingBackgroundTask> _logger;

    public AuditingBackgroundTask(
        ILifetimeScope lifetimeScope,
        ChannelReader<AuditRecord> auditsChannel,
        ILogger<AuditingBackgroundTask> logger) : base(lifetimeScope)
    {
        _auditsChannel = auditsChannel;
        _logger = logger;
    }

    protected override IAsyncEnumerable<AuditRecord> GetProducer(CancellationToken pausingToken) =>
        _auditsChannel.ReadAllAsync(pausingToken);

    protected override async Task Consume(
        AuditRecord item,
        ILifetimeScope lifetimeScope,
        CancellationToken pausingToken)
    {
        await lifetimeScope.Resolve<IAuditStorageImplementation>().StoreAudit(item);
        _logger.LogDebug("Stored audit with name {AuditName}", item.Name);
    }

    protected override async Task OnException(
        AuditRecord item,
        ILifetimeScope lifetimeScope,
        Exception exception,
        CancellationToken pausingToken)
    {
        _logger.LogError(exception, "Unexpected error while registering audit with name {AuditName}.", item.Name);
        await lifetimeScope
            .ResolveOption<AuditingExceptionHandler>()
            .IfPresentAsync(h => h(item, exception));
    }
}
