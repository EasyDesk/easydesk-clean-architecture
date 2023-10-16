using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using Microsoft.Data.SqlClient;
using Respawn;
using System.Data.Common;
using Testcontainers.MsSql;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.SqlServer;

internal class SqlServerFixtureBuilder : AbstractSqlFixtureBuilder<MsSqlContainer>
{
    private const string BackupFile = "/tmp/clean-architecture/testing/backup.bak";
    private const string BackupName = "Clean Architecture integration tests backup";
    private readonly string _databaseName;

    public SqlServerFixtureBuilder(WebServiceTestsFixtureBuilder builder, MsSqlContainer container, string databaseName)
        : base(builder, container)
    {
        _databaseName = databaseName;
    }

    protected override IDbAdapter Adapter => DbAdapter.SqlServer;

    protected override async Task BackupDatabase(ITestWebService webService, MsSqlContainer container)
    {
        await UsingDbConnection(webService, async connection =>
        {
            var commandText = $"""
                BACKUP DATABASE [{connection.Database}]
                TO DISK = {BackupFile}
                WITH NAME = {BackupName}
                """;
            await RunCommand(connection, commandText);
        });
    }

    protected override async Task RestoreBackup(ITestWebService webService, MsSqlContainer container)
    {
        await UsingDbConnection(webService, async connection =>
        {
            var commandText = $"""
                ALTER DATABASE [{connection.Database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                USE [master];
                RESTORE DATABASE [{connection.Database}]
                FROM DISK = {BackupFile}
                WITH REPLACE;
                ALTER DATABASE [{connection.Database}] SET MULTI_USER;
                """;
            await RunCommand(connection, commandText);
        });
    }

    protected async Task RunCommand(DbConnection connection, string text)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = text;
        await command.ExecuteNonQueryAsync();
    }

    protected override string GetConnectionString(MsSqlContainer container)
    {
        var originalConnectionString = container.GetConnectionString();
        var builder = new SqlConnectionStringBuilder(originalConnectionString)
        {
            InitialCatalog = _databaseName,
            TrustServerCertificate = true,
        };
        return builder.ConnectionString;
    }
}
