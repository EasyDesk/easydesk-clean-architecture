using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;

public static class SqlFixtureExtensions
{
    public static WebServiceTestsFixtureBuilder AddResettableSqlDatabase(
        this WebServiceTestsFixtureBuilder builder,
        DockerContainer container,
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

    private static async Task UsingDbConnection(ITestWebService webService, AsyncAction<DbConnection> action)
    {
        await using var scope = webService.Services.CreateAsyncScope();
        var connection = scope.ServiceProvider.GetRequiredService<DbConnection>();
        await connection.OpenAsync();
        await action(connection);
    }
}
