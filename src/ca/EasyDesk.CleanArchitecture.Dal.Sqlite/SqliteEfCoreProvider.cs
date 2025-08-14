using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Builder = Microsoft.EntityFrameworkCore.Infrastructure.SqliteDbContextOptionsBuilder;
using Extension = Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal.SqliteOptionsExtension;

namespace EasyDesk.CleanArchitecture.Dal.Sqlite;

internal class SqliteEfCoreProvider : IEfCoreProvider<Builder, Extension>
{
    private readonly Lazy<string> _connectionString;

    public SqliteEfCoreProvider(Lazy<string> connectionString)
    {
        _connectionString = connectionString;
    }

    public DbConnection NewConnection() => new SqliteConnection(_connectionString.Value);

    public void ConfigureDbProvider(DbContextOptionsBuilder options, DbConnection connection, Action<Builder> configure)
    {
        options.UseSqlite(connection, x =>
        {
            x.UseNodaTime();
            configure(x);
        });
    }
}

public static class SqliteExtensions
{
    public static IAppBuilder AddSqliteDataAccess<T>(
        this IAppBuilder builder,
        Func<string> connectionString,
        Action<EfCoreDataAccessOptions<T, Builder, Extension>>? configure = null)
        where T : AbstractDbContext
    {
        return builder.AddEfCoreDataAccess(
            new SqliteEfCoreProvider(new(connectionString)),
            configure);
    }
}
