using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Dal.SqlServer;

public static class DbContextConfigurationExtensions
{
    public static DbContextConfiguration UseSqlServer(
        this DbContextConfiguration configuration,
        Action<SqlServerDbContextOptionsBuilder> configure = null)
    {
        configuration.Options.UseSqlServer(configuration.Connection, options =>
        {
            options.MigrationsHistoryTable(tableName: "__EFMigrationsHistory", configuration.Schema);
            if (configuration.DbContextType.Assembly == typeof(DbContextConfiguration).Assembly)
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
            }
            options.UseNodaTime();
            configure?.Invoke(options);
        });
        return configuration;
    }
}
