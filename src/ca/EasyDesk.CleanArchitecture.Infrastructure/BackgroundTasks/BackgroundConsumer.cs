using Autofac;
using EasyDesk.CleanArchitecture.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;

public abstract class BackgroundConsumer<T> : PausableBackgroundService
{
    private readonly ILifetimeScope _lifetimeScope;

    public BackgroundConsumer(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    protected override sealed async Task ExecuteUntilPausedAsync(CancellationToken pausingToken)
    {
        await foreach (var t in GetProducer(pausingToken))
        {
            await using var scope = _lifetimeScope.BeginUseCaseLifetimeScope();
            try
            {
                await Consume(t, scope, pausingToken);
            }
            catch (Exception ex)
            {
                await OnException(t, scope, ex, pausingToken);
            }
        }
    }

    protected abstract IAsyncEnumerable<T> GetProducer(CancellationToken pausingToken);

    protected abstract Task Consume(T item, ILifetimeScope lifetimeScope, CancellationToken pausingToken);

    protected abstract Task OnException(T item, ILifetimeScope lifetimeScope, Exception exception, CancellationToken pausingToken);
}
