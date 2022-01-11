using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;

public class MigrationsHostedService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IEnumerable<Type> _dbContextTypes;

    public MigrationsHostedService(IServiceScopeFactory serviceScopeFactory, IEnumerable<Type> dbContextTypes)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _dbContextTypes = dbContextTypes;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            foreach (var dbContextType in _dbContextTypes)
            {
                MigrateDatabase(scope.ServiceProvider, dbContextType);
            }
        }
        return Task.CompletedTask;
    }

    private void MigrateDatabase(IServiceProvider serviceProvider, Type dbContextType)
    {
        var dbContext = serviceProvider.GetRequiredService(dbContextType) as DbContext;
        dbContext.Database.Migrate();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
