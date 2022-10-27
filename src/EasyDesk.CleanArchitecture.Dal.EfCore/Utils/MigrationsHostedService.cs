using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public class MigrationsHostedService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IEnumerable<Type> _dbContextTypes;
    private readonly ILogger<MigrationsHostedService> _logger;

    public MigrationsHostedService(
        IServiceScopeFactory serviceScopeFactory,
        IEnumerable<Type> dbContextTypes,
        ILogger<MigrationsHostedService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _dbContextTypes = dbContextTypes;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        foreach (var dbContextType in _dbContextTypes)
        {
            var dbContext = (DbContext)scope.ServiceProvider.GetService(dbContextType);
            await dbContext.Database.MigrateAsync();
            _logger.LogInformation("Successfully migrated DbContext of type {dbContextType}", dbContextType.Name);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
