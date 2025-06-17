using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;

public interface IPausableHostedService : IHostedService, IPausable;
