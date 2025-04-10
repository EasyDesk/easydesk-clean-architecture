﻿using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.SqlServer;

public static class SqlServerFixtureExtensions
{
    public static TestFixtureConfigurer AddSqlServerDatabase(
        this TestFixtureConfigurer builder,
        MsSqlContainer container,
        string databaseName,
        Action<SqlDatabaseFixtureOptions>? configureOptions = null)
    {
        string GetConnectionString(MsSqlContainer container)
        {
            var originalConnectionString = container.GetConnectionString();
            var builder = new SqlConnectionStringBuilder(originalConnectionString)
            {
                InitialCatalog = databaseName,
                TrustServerCertificate = true,
            };
            return builder.ConnectionString;
        }

        return builder.AddSqlDatabase(
            container,
            GetConnectionString,
            s => new SqlConnection(s),
            new SqlServerTableCopiesProvider(),
            configureOptions);
    }
}
