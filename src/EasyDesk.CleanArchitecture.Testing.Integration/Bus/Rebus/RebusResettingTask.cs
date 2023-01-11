using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Threading;
using Rebus.Bus;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;

public class RebusResettingTask : IPausableHostedService
{
    private readonly IBus _bus;
    private readonly IRebusPausableTaskPool _rebusPausableTaskPool;
    private readonly int _desiredNumberOfWorkers;

    public RebusResettingTask(IBus bus, IRebusPausableTaskPool rebusPausableTaskPool)
    {
        _bus = bus;
        _rebusPausableTaskPool = rebusPausableTaskPool;
        _desiredNumberOfWorkers = bus.Advanced.Workers.Count;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _bus.Advanced.Workers.SetNumberOfWorkers(_desiredNumberOfWorkers);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _bus.Advanced.Workers.SetNumberOfWorkers(0);
        return Task.CompletedTask;
    }

    public async Task Pause(CancellationToken cancellationToken)
    {
        _bus.Advanced.Workers.SetNumberOfWorkers(0);
        await _rebusPausableTaskPool.PauseAllTasks(cancellationToken);
    }

    public async Task Resume(CancellationToken cancellationToken)
    {
        await _rebusPausableTaskPool.ResumeAllTasks(cancellationToken);
        _bus.Advanced.Workers.SetNumberOfWorkers(_desiredNumberOfWorkers);
    }
}
