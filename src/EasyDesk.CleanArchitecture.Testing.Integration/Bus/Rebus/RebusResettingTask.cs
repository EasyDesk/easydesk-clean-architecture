using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using Rebus.Bus;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;

public class RebusResettingTask : IPausableHostedService
{
    private readonly IBus _bus;
    private readonly int _desiredNumberOfWorkers;

    public RebusResettingTask(IBus bus)
    {
        _bus = bus;
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

    public Task Pause(CancellationToken cancellationToken)
    {
        _bus.Advanced.Workers.SetNumberOfWorkers(0);
        return Task.CompletedTask;
    }

    public Task Resume(CancellationToken cancellationToken)
    {
        _bus.Advanced.Workers.SetNumberOfWorkers(_desiredNumberOfWorkers);
        return Task.CompletedTask;
    }
}
