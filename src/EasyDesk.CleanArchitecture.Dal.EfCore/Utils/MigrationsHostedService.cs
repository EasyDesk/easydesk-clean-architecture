using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public class MigrationsHostedService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MigrationsHostedService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            foreach (var dbContext in scope.ServiceProvider.GetServices<DbContext>())
            {
                dbContext.Database.Migrate();
            }
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
