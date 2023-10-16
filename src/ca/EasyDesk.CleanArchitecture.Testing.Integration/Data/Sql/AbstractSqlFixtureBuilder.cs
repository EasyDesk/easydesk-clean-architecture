using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using EasyDesk.Commons.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;

internal abstract class AbstractSqlFixtureBuilder<T> : ISqlDatabaseFixtureBuilder
    where T : IContainer
{
    private readonly WebServiceTestsFixtureBuilder _builder;
    private readonly T _container;
    private Func<string, string> _connectionStringModifier = It;

    public AbstractSqlFixtureBuilder(WebServiceTestsFixtureBuilder builder, T container)
    {
        _builder = builder;
        _container = container;
    }

    protected abstract IDbAdapter Adapter { get; }

    public ISqlDatabaseFixtureBuilder WithBackups()
    {
        _builder
            .OnInitialization(ws => BackupDatabase(ws, _container))
            .OnReset(ws => RestoreBackup(ws, _container));

        return this;
    }

    protected abstract Task BackupDatabase(ITestWebService webService, T container);

    protected abstract Task RestoreBackup(ITestWebService webService, T container);

    public ISqlDatabaseFixtureBuilder WithRespawn(Action<RespawnerOptionsBuilder> options)
    {
        Respawner? respawner = null;
        _builder
            .OnInitialization(ws => UsingDbConnection(ws, async connection =>
            {
                var respawnerOptionsBuilder = new RespawnerOptionsBuilder(Adapter);
                options(respawnerOptionsBuilder);
                respawner = await Respawner.CreateAsync(connection, respawnerOptionsBuilder.Build());
            }))
            .OnReset(ws => UsingDbConnection(ws, async connection =>
            {
                await respawner!.ResetAsync(connection);
            }));
        return this;
    }

    protected async Task UsingDbConnection(ITestWebService service, AsyncAction<DbConnection> action)
    {
        await using var scope = service.Services.CreateAsyncScope();
        var connection = scope.ServiceProvider.GetRequiredService<DbConnection>();
        await connection.OpenAsync();
        await action(connection);
    }

    public ISqlDatabaseFixtureBuilder ModifyConnectionString(Func<string, string> update)
    {
        _connectionStringModifier = c => update(_connectionStringModifier(c));
        return this;
    }

    public ISqlDatabaseFixtureBuilder OverrideConnectionStringFromConfiguration(string configurationKey)
    {
        _builder.WithConfiguration(config =>
        {
            var connectionString = _connectionStringModifier(GetConnectionString(_container));
            config[configurationKey] = connectionString;
        });
        return this;
    }

    protected abstract string GetConnectionString(T container);
}
