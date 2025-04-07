namespace EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;

public interface IPausable
{
    Task Pause(CancellationToken cancellationToken);

    Task Resume(CancellationToken cancellationToken);
}
