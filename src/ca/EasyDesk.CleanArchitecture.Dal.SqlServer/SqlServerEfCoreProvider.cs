using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Builder = Microsoft.EntityFrameworkCore.Infrastructure.SqlServerDbContextOptionsBuilder;
using Extension = Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerOptionsExtension;

namespace EasyDesk.CleanArchitecture.Dal.SqlServer;

internal class SqlServerEfCoreProvider : IEfCoreProvider<Builder, Extension>
{
    private readonly string _connectionString;

    public SqlServerEfCoreProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbConnection NewConnection() => new SqlConnection(_connectionString);

    public void ConfigureDbProvider(DbContextOptionsBuilder options, DbConnection connection, Action<Builder> configure)
    {
        options.UseSqlServer(connection, x =>
        {
            x.UseNodaTime();
            configure(x);
        });
    }
}

public static class SqlServerExtensions
{
    public static IAppBuilder AddSqlServerDataAccess<T>(
        this IAppBuilder builder,
        string connectionString,
        Action<EfCoreDataAccessOptions<T, Builder, Extension>>? configure = null)
        where T : AbstractDbContext
    {
#pragma warning disable EF1001
        return builder.AddEfCoreDataAccess(
            new SqlServerEfCoreProvider(connectionString),
            configure);
#pragma warning restore EF1001
    }
}
