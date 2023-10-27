using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using Testcontainers.PostgreSql;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.Postgres;

public static class PostgresFixtureExtensions
{
    public static ISqlDatabaseFixtureBuilder AddPostgresDatabase<TFixture>(
        this WebServiceTestsFixtureBuilder<TFixture> builder,
        PostgreSqlContainer container) where TFixture : ITestFixture
    {
        return builder.AddSqlDatabase(container, (b, c) => new PostgresFixtureBuilder<TFixture>(b, c));
    }
}
