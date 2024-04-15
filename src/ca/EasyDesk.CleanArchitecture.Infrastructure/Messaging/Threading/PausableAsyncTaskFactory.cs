using Microsoft.Extensions.Logging;
using Rebus.Threading;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Threading;

internal class PausableAsyncTaskFactory : IAsyncTaskFactory, IRebusPausableTaskPool
{
    private readonly ILoggerFactory _logger;
    private readonly IList<PausableAsyncTask> _tasks = new List<PausableAsyncTask>();

    public PausableAsyncTaskFactory(ILoggerFactory logger) =>
        _logger = logger;

    public IAsyncTask Create(string description, Func<Task> action, bool prettyInsignificant = false, int intervalSeconds = 10)
    {
        var task = new PausableAsyncTask(description, () => action(), _logger.CreateLogger<PausableAsyncTask>())
        {
            Interval = TimeSpan.FromSeconds(intervalSeconds),
        };
        lock (this)
        {
            _tasks.Add(task);
        }
        return task;
    }

    public async Task PauseAllTasks(CancellationToken cancellationToken)
    {
        IEnumerable<PausableAsyncTask> taskList;
        lock (this)
        {
            taskList = _tasks.ToArray();
        }
        foreach (var task in taskList)
        {
            await task.Pause(cancellationToken);
        }
    }

    public async Task ResumeAllTasks(CancellationToken cancellationToken)
    {
        IEnumerable<PausableAsyncTask> taskList;
        lock (this)
        {
            taskList = _tasks.ToArray();
        }
        foreach (var task in taskList)
        {
            await task.Resume(cancellationToken);
        }
    }
}
