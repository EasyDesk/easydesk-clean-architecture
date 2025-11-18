using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.Postgres;

public static class PostgresFixtureExtensions
{
    public static TestFixtureConfigurer AddPostgresDatabase(
        this TestFixtureConfigurer configurer,
        PostgreSqlContainer container,
        string databaseName,
        Action<SqlDatabaseFixtureOptions>? configureOptions = null)
    {
        string GetConnectionString()
        {
            var builder = new NpgsqlConnectionStringBuilder(container.GetConnectionString())
            {
                Database = databaseName,
            };
            return builder.ConnectionString;
        }
        return configurer.AddPostgresDatabase(GetConnectionString, configureOptions);
    }

    public static TestFixtureConfigurer AddPostgresDatabase(
        this TestFixtureConfigurer configurer,
        Func<string> getConnectionString,
        Action<SqlDatabaseFixtureOptions>? configureOptions = null)
    {
        return configurer.AddSqlDatabase(
            getConnectionString,
            s => new NpgsqlConnection(s),
            new PostgresTableCopiesProvider(),
            configureOptions);
    }
}
