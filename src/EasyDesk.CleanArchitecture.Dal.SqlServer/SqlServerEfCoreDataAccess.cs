using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.EfCore.Domain;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Builder = Microsoft.EntityFrameworkCore.Infrastructure.SqlServerDbContextOptionsBuilder;
using Extension = Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal.SqlServerOptionsExtension;

namespace EasyDesk.CleanArchitecture.Dal.SqlServer;

public class SqlServerEfCoreDataAccess<T> : EfCoreDataAccess<T, Builder, Extension>
    where T : DomainContext
{
    public SqlServerEfCoreDataAccess(EfCoreDataAccessOptions<T, Builder, Extension> options)
        : base(options)
    {
    }

    protected override DbConnection CreateDbConnection(string connectionString) =>
        new SqlConnection(connectionString);

    protected override void ConfigureDbProvider<C>(
        DbContextOptionsBuilder options,
        DbConnection connection,
        Action<Builder> configure)
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
    public static AppBuilder AddSqlServerDataAccess<T>(
        this AppBuilder builder,
        string connectionString,
        Action<EfCoreDataAccessOptions<T, Builder, Extension>> configure)
        where T : DomainContext
    {
#pragma warning disable EF1001
        return builder.AddEfCoreDataAccess(
            connectionString,
            options => new SqlServerEfCoreDataAccess<T>(options),
            configure);
#pragma warning restore EF1001
    }
}
