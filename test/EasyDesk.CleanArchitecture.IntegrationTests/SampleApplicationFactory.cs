using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using EasyDesk.SampleApp.Web.Controllers.V_1_0;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

public class SampleApplicationFactory : WebApplicationFactory<PersonController>, IAsyncLifetime
{
    private readonly PostgreSqlTestcontainer _dbContainer;
    private readonly RabbitMqTestcontainer _rabbitMqContainer;

    public SampleApplicationFactory()
    {
        _dbContainer = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "TestSampleDb",
                Username = "sample",
                Password = "sample"
            })
            .Build();

        _rabbitMqContainer = new TestcontainersBuilder<RabbitMqTestcontainer>()
            .WithMessageBroker(new RabbitMqTestcontainerConfiguration
            {
                Username = "guest",
                Password = "guest"
            })
            .Build();
    }

    public string RabbitMqConnectionString => _rabbitMqContainer.ConnectionString;

    public string PostgresConnectionString => _dbContainer.ConnectionString;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["ConnectionStrings:RabbitMq"] = RabbitMqConnectionString,
            ["ConnectionStrings:MainDb"] = PostgresConnectionString,
        }));
        return base.CreateHost(builder);
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
    }
}
