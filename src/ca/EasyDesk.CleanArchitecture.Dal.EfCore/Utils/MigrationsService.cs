using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public class MigrationsService
{
    private readonly IEnumerable<DbContext> _contexts;
    private readonly ILogger<MigrationsService> _logger;

    internal MigrationsService(IEnumerable<DbContext> contexts, ILogger<MigrationsService> logger)
    {
        _contexts = contexts;
        _logger = logger;
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
        foreach (var context in _contexts)
        {
            await context.Database.MigrateAsync();
            _logger.LogInformation("Successfully asynchronously migrated DbContext of type {contextType}", context.GetType().Name);
        }
    }

    public void MigrateSync()
    {
        foreach (var context in _contexts)
        {
            context.Database.Migrate();
            _logger.LogInformation("Successfully synchronously migrated DbContext of type {contextType}", context.GetType().Name);
        }
    }
}
