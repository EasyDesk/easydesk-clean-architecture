using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Rebus.Persistence.InMem;
using Rebus.Subscriptions;
using Rebus.Transport.InMem;
using Respawn;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

public class SampleApplicationFactory : IntegrationTestsWebApplicationFactory<PersonController>
{
    private readonly PostgreSqlTestcontainer _postgres;
    private readonly InMemNetwork _network = new();
    private readonly InMemorySubscriberStore _subscriberStore = new();

    private DbConnection _dbConnection;
    private Respawner _respawner;

    public SampleApplicationFactory()
    {
        _postgres = RegisterTestContainer<PostgreSqlTestcontainer>(container => container
            .WithName($"{nameof(SampleApplicationFactory)}-integration-tests-postgres")
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "TestSampleDb",
                Username = "sample",
                Password = "sample"
            }));
    }

    protected override void ConfigureConfiguration(IConfigurationBuilder config)
    {
        config.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["ConnectionStrings:MainDb"] = _postgres.ConnectionString,
        });
    }

    protected override void OverrideServices(IServiceCollection services)
    {
        services.RemoveAll<RebusTransportConfiguration>();
        services.AddSingleton<RebusTransportConfiguration>((t, e) =>
        {
            t.UseInMemoryTransport(_network, e);
            t.OtherService<ISubscriptionStorage>().StoreInMemory(_subscriberStore);
        });
    }

    public async Task ResetPersistedData()
    {
        await _respawner.ResetAsync(_dbConnection);
        _network.Reset();
        _subscriberStore.Reset();
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
