using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;
using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.SampleApp.Web;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using Microsoft.Data.SqlClient;
using Respawn;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

public class SampleAppTestsFixture : WebServiceTestsFixture
{
    public SampleAppTestsFixture() : base(typeof(PersonController))
    {
    }

    protected override void ConfigureFixture(WebServiceTestsFixtureBuilder builder)
    {
        using var dbConfiguration = new MsSqlTestcontainerConfiguration
        {
            Database = "TestSampleDb",
            Password = "sample.123",
        };
        var container = new TestcontainersBuilder<MsSqlTestcontainer>()
            .WithUniqueName("sample-app-integration-tests-detabbes")
            .WithDatabase(dbConfiguration)
            .Build();

        builder
            .WithConfiguration("DbProvider", DbProvider.SqlServer.ToString())
            .AddInMemoryRebus()
            .AddResettableSqlDatabase(
                container,
                "ConnectionStrings:SqlServer",
                new RespawnerOptions
                {
                    DbAdapter = DbAdapter.SqlServer,
                    SchemasToInclude = new[] { "domain", "messaging", "auth", "sagas", "audit" }
                },
                c => new SqlConnectionStringBuilder(c) { TrustServerCertificate = true }.ConnectionString);
    }

    ////protected override void ConfigureFixture(WebServiceTestsFixtureBuilder builder)
    ////{
    ////    using var dbConfiguration = new PostgreSqlTestcontainerConfiguration
    ////    {
    ////        Database = "TestSampleDb",
    ////        Username = "sample",
    ////        Password = "sample",
    ////    };
    ////    var container = new TestcontainersBuilder<PostgreSqlTestcontainer>()
    ////        .WithUniqueName("sample-app-integration-tests-postgres")
    ////        .WithDatabase(dbConfiguration)
    ////        .Build();

    ////    builder
    ////        .WithConfiguration("DbProvider", DbProvider.PostgreSql.ToString())
    ////        .AddInMemoryRebus()
    ////        .AddResettableSqlDatabase(
    ////            container,
    ////            "ConnectionStrings:PostgreSql",
    ////            new RespawnerOptions
    ////            {
    ////                DbAdapter = DbAdapter.Postgres,
    ////                SchemasToInclude = new[] { "domain", "messaging", "auth", "sagas", "audit" }
    ////            },
    ////            connectionString =>
    ////            {
    ////                var csb = new NpgsqlConnectionStringBuilder(connectionString)
    ////                {
    ////                    IncludeErrorDetail = true
    ////                };
    ////                return csb.ConnectionString;
    ////            });
    ////}
}
