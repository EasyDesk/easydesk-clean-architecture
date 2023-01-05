using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Testing.Integration;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;
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

public class SampleAppTestsFixture : WebServiceTestsFixture
{
    private readonly PostgreSqlTestcontainer _postgres;
    private readonly InMemNetwork _network = new();
    private readonly InMemorySubscriberStore _subscriberStore = new();

    private DbConnection _dbConnection;
    private Respawner _respawner;

    public SampleAppTestsFixture()
    {
        _postgres = RegisterTestContainer<PostgreSqlTestcontainer>(container => container
            .WithName($"{nameof(SampleAppTestsFixture)}-integration-tests-postgres")
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "TestSampleDb",
                Username = "sample",
                Password = "sample",
            }));
    }

    protected override Type WebServiceEntryPointMarker => typeof(PersonController);

    protected override void ConfigureWebService(TestWebServiceBuilder builder)
    {
        builder
            .WithConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:MainDb"] = _postgres.ConnectionString,
                });
            })
            .WithServices(services =>
            {
                services.RemoveAll<RebusTransportConfiguration>();
                services.AddSingleton<RebusTransportConfiguration>((t, e) =>
                {
                    t.UseInMemoryTransport(_network, e);
                    t.OtherService<ISubscriptionStorage>().StoreInMemory(_subscriberStore);
                });
                services.AddHostedService<RebusResettingTask>();
            });
    }

    protected override async Task OnReset()
    {
        await _respawner.ResetAsync(_dbConnection);
        _network.Reset();
        _subscriberStore.Reset();
    }

    protected override async Task OnInitialization()
    {
        _dbConnection = new NpgsqlConnection(_postgres.ConnectionString);
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "domain", "messaging" }
        });
    }

    protected override async Task OnDisposal()
    {
        await _dbConnection.DisposeAsync();
    }
}
