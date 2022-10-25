using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using EasyDesk.SampleApp.Web.Controllers.V_1_0;
using Microsoft.Extensions.Configuration;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

public class SampleApplicationFactory : IntegrationTestsWebApplicationFactory<PersonController>
{
    private readonly PostgreSqlTestcontainer _dbContainer;
    private readonly RabbitMqTestcontainer _rabbitMqContainer;

    public SampleApplicationFactory()
    {
        _dbContainer = RegisterTestContainer<PostgreSqlTestcontainer>(container => container
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "TestSampleDb",
                Username = "sample",
                Password = "sample"
            }));

        _rabbitMqContainer = RegisterTestContainer<RabbitMqTestcontainer>(container => container
            .WithMessageBroker(new RabbitMqTestcontainerConfiguration
            {
                Username = "guest",
                Password = "guest"
            }));
    }

    public string RabbitMqConnectionString => _rabbitMqContainer.ConnectionString;

    public string PostgresConnectionString => _dbContainer.ConnectionString;

    protected override void ConfigureConfiguration(IConfigurationBuilder config)
    {
        config.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["ConnectionStrings:RabbitMq"] = RabbitMqConnectionString,
            ["ConnectionStrings:MainDb"] = PostgresConnectionString,
        });
    }
}
