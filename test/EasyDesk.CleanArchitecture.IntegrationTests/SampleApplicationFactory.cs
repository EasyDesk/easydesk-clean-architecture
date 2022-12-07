using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

public class SampleApplicationFactory : IntegrationTestsWebApplicationFactory<PersonController>
{
    private readonly PostgreSqlTestcontainer _postgres;
    private readonly RabbitMqTestcontainer _rabbitMq;

    private DbConnection _dbConnection;
    private Respawner _respawner;

    public SampleApplicationFactory()
    {
        _postgres = RegisterTestContainer<PostgreSqlTestcontainer>(container => container
            .WithName("integration-tests-postgres")
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "TestSampleDb",
                Username = "sample",
                Password = "sample"
            }));

        _rabbitMq = RegisterTestContainer<RabbitMqTestcontainer>(container => container
            .WithName("integration-tests-rabbitmq")
            .WithMessageBroker(new RabbitMqTestcontainerConfiguration
            {
                Username = "guest",
                Password = "guest"
            }));
    }

    protected override void ConfigureConfiguration(IConfigurationBuilder config)
    {
        config.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["ConnectionStrings:RabbitMq"] = _rabbitMq.ConnectionString,
            ["ConnectionStrings:MainDb"] = _postgres.ConnectionString,
        });
    }

    public async Task ResetDatabase()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _dbConnection = new NpgsqlConnection(_postgres.ConnectionString);
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "domain", "messaging" }
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _dbConnection.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
