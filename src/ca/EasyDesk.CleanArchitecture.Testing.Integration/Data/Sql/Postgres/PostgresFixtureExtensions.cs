using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using Testcontainers.PostgreSql;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.Postgres;

public static class PostgresFixtureExtensions
{
    public static ISqlDatabaseFixtureBuilder AddPostgresDatabase(
        this WebServiceTestsFixtureBuilder builder,
        PostgreSqlContainer container)
    {
        return builder.AddSqlDatabase(container, (b, c) => new PostgresFixtureBuilder(b, c));
    }
}
