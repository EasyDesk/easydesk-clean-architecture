using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Containers;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

public class SampleAppContainersContext : ContainersContext
{
    public PostgreSqlTestcontainer Postgres { get; }

    public RabbitMqTestcontainer RabbitMq { get; }

    public SampleAppContainersContext()
    {
        Postgres = RegisterTestContainer<PostgreSqlTestcontainer>(container => container
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "TestSampleDb",
                Username = "sample",
                Password = "sample"
            }));

        RabbitMq = RegisterTestContainer<RabbitMqTestcontainer>(container => container
            .WithMessageBroker(new RabbitMqTestcontainerConfiguration
            {
                Username = "guest",
                Password = "guest"
            }));
    }
}
