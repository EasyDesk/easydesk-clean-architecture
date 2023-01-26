using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;

public abstract class PausableBackgroundService : BackgroundService, IPausableHostedService
{
    private bool _isPaused = false;
#pragma warning disable CA2213 // Disposable fields should be disposed
    private CancellationTokenSource? _pause;
    private CancellationTokenSource? _resume;
#pragma warning restore CA2213 // Disposable fields should be disposed
    private Task? _executingTask;

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
            finally
            {
                _pause?.Dispose();
            }

            try
            {
                lock (this)
                {
                    _isPaused = true;
                    _resume = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                }
                await WaitUntilResumed(_resume.Token);
            }
            finally
            {
                _resume?.Dispose();
            }
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

            executingTask = _executingTask ?? throw new InvalidOperationException(
                $"The background task is not running. {nameof(ExecuteAsync)} must be executed before {nameof(Pause)}.");

            _pause.Cancel();
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
                if (_resume is null)
                {
                    throw new InvalidOperationException(
                        $"The background task is not paused. {nameof(ExecuteAsync)} must be executed before {nameof(Pause)}, which must be executed before {nameof(Resume)}.");
                }
                _resume.Cancel();
            }
        }
        return Task.CompletedTask;
    }
}
