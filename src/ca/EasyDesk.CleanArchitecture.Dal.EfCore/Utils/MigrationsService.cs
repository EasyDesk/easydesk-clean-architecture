using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public class MigrationsService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<Type> _dbContextTypes;
    private readonly ILogger<MigrationsService> _logger;

    internal MigrationsService(IServiceProvider serviceProvider, IEnumerable<Type> dbContextTypes)
    {
        _serviceProvider = serviceProvider;
        _dbContextTypes = dbContextTypes;
        _logger = serviceProvider.GetRequiredService<ILogger<MigrationsService>>();
    }

    public async Task Migrate(bool sync)
    {
        if (sync)
        {
            MigrateSync();
        }
        else
        {
            await MigrateAsync();
        }
    }

    public async Task MigrateAsync()
    {
        foreach (var dbContextType in _dbContextTypes)
        {
            var dbContext = (DbContext)_serviceProvider.GetRequiredService(dbContextType);
            await dbContext.Database.MigrateAsync();
            _logger.LogInformation("Successfully asynchronously migrated DbContext of type {dbContextType}", dbContextType.Name);
        }
    }

    public void MigrateSync()
    {
        foreach (var dbContextType in _dbContextTypes)
        {
            var dbContext = (DbContext)_serviceProvider.GetRequiredService(dbContextType);
            dbContext.Database.Migrate();
            _logger.LogInformation("Successfully synchronously migrated DbContext of type {dbContextType}", dbContextType.Name);
        }
    }
}
