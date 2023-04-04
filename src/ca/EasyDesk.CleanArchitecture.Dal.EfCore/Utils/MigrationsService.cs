using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

internal class MigrationsService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<Type> _dbContextTypes;
    private readonly ILogger<MigrationsService> _logger;

    public MigrationsService(IServiceProvider serviceProvider, IEnumerable<Type> dbContextTypes)
    {
        _serviceProvider = serviceProvider;
        _dbContextTypes = dbContextTypes;
        _logger = serviceProvider.GetRequiredService<ILogger<MigrationsService>>();
    }

    public async Task MigrateDatabases()
    {
        foreach (var dbContextType in _dbContextTypes)
        {
            var dbContext = (DbContext)_serviceProvider.GetRequiredService(dbContextType)!;
            await dbContext.Database.MigrateAsync();
            _logger.LogInformation("Successfully migrated DbContext of type {dbContextType}", dbContextType.Name);
        }
    }
}
