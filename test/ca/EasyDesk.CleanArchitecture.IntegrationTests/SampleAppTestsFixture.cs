using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Dal.EfCore.Auditing;
using EasyDesk.CleanArchitecture.Dal.EfCore.Authorization;
using EasyDesk.CleanArchitecture.Dal.EfCore.Domain;
using EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;
using EasyDesk.CleanArchitecture.Dal.EfCore.Sagas;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;
using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.SampleApp.Web;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using Microsoft.Data.SqlClient;
using Npgsql;
using Respawn;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

public class SampleAppTestsFixture : WebServiceTestsFixture
{
    private static readonly RespawnerOptions _respawnerOptions = new RespawnerOptions
    {
        DbAdapter = DbAdapter.SqlServer,
        SchemasToInclude = new[]
        {
            DomainContext.SchemaName,
            MessagingModel.SchemaName,
            AuthorizationModel.SchemaName,
            SagaManagerModel.SchemaName,
            AuditModel.SchemaName
        }
    };

    private static readonly Dictionary<DbProvider, Action<WebServiceTestsFixtureBuilder>> _providerConfigs = new()
    {
        { DbProvider.SqlServer, ConfigureSqlServer },
        { DbProvider.PostgreSql, ConfigurePostgreSql },
    };

    public SampleAppTestsFixture() : base(typeof(PersonController))
    {
    }

    protected override void ConfigureFixture(WebServiceTestsFixtureBuilder builder)
    {
        builder.AddInMemoryRebus();
        ConfigureForDbProvider(DbProvider.SqlServer, builder);
    }

    private void ConfigureForDbProvider(DbProvider provider, WebServiceTestsFixtureBuilder builder)
    {
        builder.WithConfiguration("DbProvider", provider.ToString());
        _providerConfigs[provider](builder);
    }

    private static void ConfigureSqlServer(WebServiceTestsFixtureBuilder builder)
    {
        using var dbConfiguration = new MsSqlTestcontainerConfiguration
        {
            Database = "TestSampleDb",
            Password = "sample.123",
        };
        var container = new TestcontainersBuilder<MsSqlTestcontainer>()
            .WithUniqueName("sample-app-integration-tests-sqlserver")
            .WithDatabase(dbConfiguration)
            .Build();

        builder.AddResettableSqlDatabase(
            container,
            "ConnectionStrings:SqlServer",
            _respawnerOptions,
            c => new SqlConnectionStringBuilder(c) { TrustServerCertificate = true }.ConnectionString);
    }

    private static void ConfigurePostgreSql(WebServiceTestsFixtureBuilder builder)
    {
        using var dbConfiguration = new PostgreSqlTestcontainerConfiguration
        {
            Database = "TestSampleDb",
            Username = "sample",
            Password = "sample",
        };
        var container = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithUniqueName("sample-app-integration-tests-postgres")
            .WithDatabase(dbConfiguration)
            .Build();

        builder.AddResettableSqlDatabase(
            container,
            "ConnectionStrings:PostgreSql",
            _respawnerOptions,
            connectionString => new NpgsqlConnectionStringBuilder(connectionString) { IncludeErrorDetail = true }.ConnectionString);
    }
}
