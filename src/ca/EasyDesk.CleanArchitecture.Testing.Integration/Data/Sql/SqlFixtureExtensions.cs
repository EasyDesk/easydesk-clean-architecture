using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using EasyDesk.Commons.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using System.Data.Common;
using Testcontainers.MsSql;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;

public static class SqlFixtureExtensions
{
    private const string BackupFile = "/tmp/clean-architecture/testing/backup.bak";
    private const string BackupName = "Clean Architecture integration tests backup";

    public static WebServiceTestsFixtureBuilder AddSqlDatabaseWithReset(
        this WebServiceTestsFixtureBuilder builder,
        IContainer container,
        RespawnerOptions respawnerOptions)
    {
        Respawner? respawner = null;
        return builder
            .ConfigureContainers(containers => containers.RegisterTestContainer(container))
            .OnInitialization(ws => UsingDbConnection(ws, async connection =>
            {
                respawner = await Respawner.CreateAsync(connection, respawnerOptions);
            }))
            .OnReset(ws => UsingDbConnection(ws, async connection =>
            {
                await respawner!.ResetAsync(connection);
            }));
    }

    public static WebServiceTestsFixtureBuilder AddSqlServerDatabaseWithBackups(
        this WebServiceTestsFixtureBuilder builder,
        MsSqlContainer container)
    {
        return builder
            .ConfigureContainers(containers => containers.RegisterTestContainer(container))
            .OnInitialization(ws => UsingDbConnection(ws, async connection =>
            {
                await connection.RunCommand(
                    $"""
                    BACKUP DATABASE [{connection.Database}]
                    TO DISK = @backupFile
                    WITH NAME = @backupName;
                    """,
                    new SqlParameter("@backupFile", BackupFile),
                    new SqlParameter("@backupName", BackupName));
            }))
            .OnReset(ws => UsingDbConnection(ws, async connection =>
            {
                await connection.RunCommand(
                    $"""
                    ALTER DATABASE [{connection.Database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    USE [master];
                    RESTORE DATABASE [{connection.Database}]
                    FROM DISK = @backupFile
                    WITH REPLACE;
                    ALTER DATABASE [{connection.Database}] SET MULTI_USER;
                    """,
                    new SqlParameter("@backupFile", BackupFile));
            }));
    }

    private static async Task UsingDbConnection(ITestWebService service, AsyncAction<DbConnection> action)
    {
        await using var scope = service.Services.CreateAsyncScope();
        var connection = scope.ServiceProvider.GetRequiredService<DbConnection>();
        await connection.OpenAsync();
        await action(connection);
    }

    private static async Task RunCommand(this DbConnection connection, string text, params SqlParameter[] parameters)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = text;
        command.Parameters.AddRange(parameters);
        await command.ExecuteNonQueryAsync();
    }
}
