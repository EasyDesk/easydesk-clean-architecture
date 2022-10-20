using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql;

public static class DbContextConfigurationExtensions
{
    public static DbContextConfiguration UsePostgreSql(
        this DbContextConfiguration configuration,
        Action<NpgsqlDbContextOptionsBuilder> configure = null)
    {
        configuration.Options.UseNpgsql(configuration.Connection, options =>
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
