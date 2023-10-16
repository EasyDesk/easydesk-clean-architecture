using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using Respawn;
using Testcontainers.PostgreSql;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.Postgres;

internal class PostgresFixtureBuilder : AbstractSqlFixtureBuilder<PostgreSqlContainer>
{
    public PostgresFixtureBuilder(WebServiceTestsFixtureBuilder builder, PostgreSqlContainer container) : base(builder, container)
    {
    }

    protected override IDbAdapter Adapter => DbAdapter.Postgres;

    protected override Task BackupDatabase(ITestWebService webService, PostgreSqlContainer container) =>
        throw new NotImplementedException();

    protected override Task RestoreBackup(ITestWebService webService, PostgreSqlContainer container) =>
        throw new NotImplementedException();

    protected override string GetConnectionString(PostgreSqlContainer container) => throw new NotImplementedException();
}
