using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;
using EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using Npgsql;
using Respawn;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

public class SampleAppTestsFixture : WebServiceTestsFixture
{
    public SampleAppTestsFixture() : base(typeof(PersonController))
    {
    }

    protected override void ConfigureFixture(WebServiceTestsFixtureBuilder builder)
    {
        var container = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithName("sample-app-integration-tests-postgres")
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "TestSampleDb",
                Username = "sample",
                Password = "sample",
            })
            .Build();

        builder
            .AddInMemoryRebus()
            .AddResettableSqlDatabase(
                container,
                "ConnectionStrings:MainDb",
                new RespawnerOptions
                {
                    DbAdapter = DbAdapter.Postgres
                },
                connectionString =>
                {
                    var csb = new NpgsqlConnectionStringBuilder(connectionString);
                    csb.IncludeErrorDetail = true;
                    return csb.ConnectionString;
                });
    }
}
