using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;

public abstract class PausableBackgroundService : BackgroundService, IPausableHostedService
{
    private bool _isPaused = false;
    private CancellationTokenSource _pause;
    private CancellationTokenSource _resume;
    private Task _executingTask;

    protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                lock (this)
                {
                    _isPaused = false;
                    _pause = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    _executingTask = ExecuteUntilPausedAsync(_pause.Token);
                }
                await _executingTask;
            }
            catch (OperationCanceledException)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
            }
            lock (this)
            {
                _isPaused = true;
                _resume = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            }
            await WaitUntilResumed(_resume.Token);
        }
    }

    private async Task WaitUntilResumed(CancellationToken resumeToken)
    {
        try
        {
            await Task.Delay(Timeout.Infinite, resumeToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    protected abstract Task ExecuteUntilPausedAsync(CancellationToken pausingToken);

    public async Task Pause(CancellationToken cancellationToken)
    {
        Task executingTask;
        lock (this)
        {
            if (_isPaused)
            {
                return;
            }

            if (_pause is null)
            {
                return;
            }

            _pause.Cancel();

            executingTask = _executingTask;
        }
        try
        {
            await Task.WhenAny(executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
        catch
        {
        }
    }

    public Task Resume(CancellationToken cancellationToken)
    {
        lock (this)
        {
            if (_isPaused)
            {
                _resume.Cancel();
            }
        }
        return Task.CompletedTask;
    }
}
