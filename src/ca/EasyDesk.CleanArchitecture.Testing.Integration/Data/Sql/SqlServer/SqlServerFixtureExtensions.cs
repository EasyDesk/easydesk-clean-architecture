using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using Testcontainers.MsSql;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.SqlServer;

public static class SqlServerFixtureExtensions
{
    public static ISqlDatabaseFixtureBuilder AddSqlServerDatabase<TFixture>(
        this WebServiceTestsFixtureBuilder<TFixture> builder,
        MsSqlContainer container,
        string databaseName) where TFixture : WebServiceTestsFixture<TFixture>
    {
        return builder.AddSqlDatabase(container, (b, c) => new SqlServerFixtureBuilder<TFixture>(b, c, databaseName));
    }
}
