using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using Microsoft.Extensions.Logging;
using Rebus.Threading;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Threading;

internal class PausableAsyncTask : PausableBackgroundService, IAsyncTask
{
    public static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan MinimumInterval = TimeSpan.FromMilliseconds(100);

    private readonly ILogger _logger;
    private TimeSpan _interval;
    private readonly string _description;
    private readonly AsyncAction _action;

    public PausableAsyncTask(string description, AsyncAction action, ILogger<PausableAsyncTask> logger)
    {
        _description = description;
        _action = action;
        _logger = logger;
        Interval = DefaultInterval;
    }

    public TimeSpan Interval
    {
        get => _interval;
        set => _interval = value < MinimumInterval
            ? MinimumInterval
            : value;
    }

    protected override async Task ExecuteUntilPausedAsync(CancellationToken pausingToken)
    {
        while (!pausingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(Interval, pausingToken);
                await _action();
            }
            catch (OperationCanceledException) when (pausingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in periodic task {Description}", _description);
            }
        }
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting periodic task {TaskDescription} with interval {TimerInterval}", _description, Interval);
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Stopping periodic task {TaskDescription}", _description);
        return base.StopAsync(cancellationToken);
    }

    public void Start()
    {
        Task.Run(() => StartAsync(CancellationToken.None));
    }
}
