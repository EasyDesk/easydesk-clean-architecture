using Autofac;
using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using EasyDesk.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

public delegate Task OutboxExceptionHandler(Exception exception);

internal class OutboxConsumer : BackgroundConsumer<Nothing>
{
    private readonly OutboxFlushRequestsChannel _requestsChannel;
    private readonly ILogger<OutboxConsumer> _logger;

    public OutboxConsumer(
        ILifetimeScope lifetimeScope,
        OutboxFlushRequestsChannel requestsChannel,
        ILogger<OutboxConsumer> logger) : base(lifetimeScope)
    {
        _requestsChannel = requestsChannel;
        _logger = logger;
    }

    protected override IAsyncEnumerable<Nothing> GetProducer(CancellationToken pausingToken) =>
        _requestsChannel.GetAllFlushRequests(pausingToken);

    protected override async Task Consume(Nothing item, ILifetimeScope lifetimeScope, CancellationToken pausingToken)
    {
        await lifetimeScope.Resolve<OutboxFlusher>().Flush();
        _logger.LogDebug("Correctly flushed outbox");
    }

    protected override async Task OnException(Nothing item, ILifetimeScope lifetimeScope, Exception exception, CancellationToken pausingToken)
    {
        _logger.LogError(exception, "Unexpected error while flushing the outbox.");
        await lifetimeScope
            .ResolveOption<OutboxExceptionHandler>()
            .IfPresentAsync(h => h(exception));
    }
}
