using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;

public abstract class BackgroundConsumer<T> : PausableBackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BackgroundConsumer(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override sealed async Task ExecuteUntilPausedAsync(CancellationToken pausingToken)
    {
        await foreach (var t in GetProducer(pausingToken))
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            try
            {
                await Consume(t, scope.ServiceProvider, pausingToken);
            }
            catch (Exception ex)
            {
                await OnException(t, scope.ServiceProvider, ex, pausingToken);
            }
        }
    }

    protected abstract IAsyncEnumerable<T> GetProducer(CancellationToken pausingToken);

    protected abstract Task Consume(T item, IServiceProvider serviceProvider, CancellationToken pausingToken);

    protected abstract Task OnException(T item, IServiceProvider serviceProvider, Exception exception, CancellationToken pausingToken);
}
