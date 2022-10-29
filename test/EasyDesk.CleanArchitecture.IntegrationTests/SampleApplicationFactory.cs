using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using EasyDesk.SampleApp.Web.Controllers.V_1_0;
using Microsoft.Extensions.Configuration;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

public class SampleApplicationFactory : IntegrationTestsWebApplicationFactory<PersonController>
{
    private readonly PostgreSqlTestcontainer _postgres;
    private readonly RabbitMqTestcontainer _rabbitMq;

    public SampleApplicationFactory()
    {
        _postgres = RegisterTestContainer<PostgreSqlTestcontainer>(container => container
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "TestSampleDb",
                Username = "sample",
                Password = "sample"
            }));

        _rabbitMq = RegisterTestContainer<RabbitMqTestcontainer>(container => container
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
}
