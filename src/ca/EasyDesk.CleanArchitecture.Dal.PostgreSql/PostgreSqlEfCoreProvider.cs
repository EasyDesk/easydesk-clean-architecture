using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data.Common;
using Builder = Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.NpgsqlDbContextOptionsBuilder;
using Extension = Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal.NpgsqlOptionsExtension;

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql;

internal class PostgreSqlEfCoreProvider : IEfCoreProvider<Builder, Extension>
{
    private readonly Lazy<NpgsqlDataSource> _dataSource;

    public PostgreSqlEfCoreProvider(Lazy<string> connectionString)
    {
        _dataSource = new(() =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString.Value);
            dataSourceBuilder.UseNodaTime();
            return dataSourceBuilder.Build();
        });
    }

    public DbConnection NewConnection() => _dataSource.Value.CreateConnection();

    public void ConfigureDbProvider(DbContextOptionsBuilder options, DbConnection connection, Action<Builder> configure)
    {
        options.UseNpgsql(connection, x =>
        {
            x.UseNodaTime();
            configure(x);
        });
    }
}

public static class SqlServerExtensions
{
    public static IAppBuilder AddPostgreSqlDataAccess<T>(
        this IAppBuilder builder,
        Func<string> connectionString,
        Action<EfCoreDataAccessOptions<T, Builder, Extension>>? configure = null)
        where T : AbstractDbContext
    {
#pragma warning disable EF1001
        return builder.AddEfCoreDataAccess(
            new PostgreSqlEfCoreProvider(new(connectionString)),
            configure);
#pragma warning restore EF1001
    }
}
