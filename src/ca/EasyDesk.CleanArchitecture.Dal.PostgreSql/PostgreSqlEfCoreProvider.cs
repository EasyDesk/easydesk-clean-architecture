﻿using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
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
    private readonly NpgsqlDataSource _dataSource;

    public PostgreSqlEfCoreProvider(string connectionString)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseNodaTime();
        _dataSource = dataSourceBuilder.Build();
    }

    public DbConnection NewConnection() => _dataSource.CreateConnection();

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
        string connectionString,
        Action<EfCoreDataAccessOptions<T, Builder, Extension>>? configure = null)
        where T : AbstractDbContext
    {
#pragma warning disable EF1001
        return builder.AddEfCoreDataAccess(
            new PostgreSqlEfCoreProvider(connectionString),
            configure);
#pragma warning restore EF1001
    }
}
