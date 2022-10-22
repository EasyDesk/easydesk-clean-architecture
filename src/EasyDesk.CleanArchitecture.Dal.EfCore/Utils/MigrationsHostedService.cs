using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public class MigrationsHostedService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IEnumerable<Type> _dbContextTypes;

    public MigrationsHostedService(IServiceScopeFactory serviceScopeFactory, IEnumerable<Type> dbContextTypes)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _dbContextTypes = dbContextTypes;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        foreach (var dbContextType in _dbContextTypes)
        {
            var dbContext = (DbContext)scope.ServiceProvider.GetService(dbContextType);
            await dbContext.Database.MigrateAsync();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
