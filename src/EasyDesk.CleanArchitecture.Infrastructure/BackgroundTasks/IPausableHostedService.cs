using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;

public interface IPausableHostedService : IHostedService
{
    Task Pause(CancellationToken cancellationToken);

    Task Resume(CancellationToken cancellationToken);
}
