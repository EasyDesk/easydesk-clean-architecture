using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using Testcontainers.MsSql;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.SqlServer;

public static class SqlServerFixtureExtensions
{
    public static ISqlDatabaseFixtureBuilder AddSqlServerDatabase(
        this WebServiceTestsFixtureBuilder builder,
        MsSqlContainer container,
        string databaseName)
    {
        return builder.AddSqlDatabase(container, (b, c) => new SqlServerFixtureBuilder(b, c, databaseName));
    }
}
