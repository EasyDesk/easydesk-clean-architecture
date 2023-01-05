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
            _isPaused = false;
            _pause = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

            try
            {
                _executingTask = ExecuteUntilPausedAsync(_pause.Token);
                await _executingTask;
            }
            catch
            {
                _executingTask = Task.CompletedTask;
                if (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
            }

            _isPaused = true;
            _resume = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

            await WaitUntilResumed(_resume.Token);

            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private async Task WaitUntilResumed(CancellationToken resumeToken)
    {
        try
        {
            await Task.Delay(Timeout.Infinite, resumeToken);
        }
        catch (TaskCanceledException)
        {
        }
    }

    protected abstract Task ExecuteUntilPausedAsync(CancellationToken pausingToken);

    public async Task Pause(CancellationToken cancellationToken)
    {
        if (_isPaused)
        {
            return;
        }

        _pause.Cancel();
        try
        {
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
        catch
        {
        }
    }

    public Task Resume(CancellationToken cancellationToken)
    {
        if (_isPaused)
        {
            _resume.Cancel();
        }
        return Task.CompletedTask;
    }
}
