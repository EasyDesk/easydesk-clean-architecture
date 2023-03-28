namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Threading;

public interface IRebusPausableTaskPool
{
    Task PauseAllTasks(CancellationToken cancellationToken);

    Task ResumeAllTasks(CancellationToken cancellationToken);
}
