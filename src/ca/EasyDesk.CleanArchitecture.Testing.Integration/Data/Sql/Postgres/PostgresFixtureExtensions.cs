using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.Postgres;

public static class PostgresFixtureExtensions
{
    public static TestFixtureConfigurer AddPostgresDatabase(
        this TestFixtureConfigurer configurer,
        PostgreSqlContainer container,
        Action<SqlDatabaseFixtureOptions>? configureOptions = null)
    {
        return configurer.AddSqlDatabase(
            container,
            c => c.GetConnectionString(),
            s => new NpgsqlConnection(s),
            new PostgresTableCopiesProvider(),
            configureOptions);
    }
}
