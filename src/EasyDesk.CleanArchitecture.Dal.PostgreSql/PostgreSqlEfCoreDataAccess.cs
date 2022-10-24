using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.EfCore.Domain;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data.Common;
using Builder = Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.NpgsqlDbContextOptionsBuilder;
using Extension = Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal.NpgsqlOptionsExtension;

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql;

public class PostgreSqlEfCoreDataAccess<T> : EfCoreDataAccess<T, Builder, Extension>
    where T : DomainContext<T>
{
    public PostgreSqlEfCoreDataAccess(EfCoreDataAccessOptions<T, Builder, Extension> options)
        : base(options)
    {
    }

    protected override DbConnection CreateDbConnection(string connectionString) =>
        new NpgsqlConnection(connectionString);

    protected override void ConfigureDbProvider(
        DbContextOptionsBuilder options,
        DbConnection connection,
        Action<Builder> configure)
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
    public static AppBuilder AddPostgreSqlDataAccess<T>(
        this AppBuilder builder,
        string connectionString,
        Action<EfCoreDataAccessOptions<T, Builder, Extension>> configure = null)
        where T : DomainContext<T>
    {
#pragma warning disable EF1001
        return builder.AddEfCoreDataAccess(
            connectionString,
            options => new PostgreSqlEfCoreDataAccess<T>(options),
            configure);
#pragma warning restore EF1001
    }
}
