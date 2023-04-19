using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;
using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.Commons.Utils;
using EasyDesk.SampleApp.Web;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using Microsoft.Data.SqlClient;
using Npgsql;
using Respawn;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

public class SampleAppTestsFixture : WebServiceTestsFixture
{
    public const DbProvider DefaultDbProvider = DbProvider.PostgreSql;

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

        var provider = Environment.GetEnvironmentVariable("DB_PROVIDER")
            .AsOption()
            .Map(p => Enums.ParseOption<DbProvider>(p).OrElseThrow(() => new Exception("Invalid DB provider")))
            .OrElse(DefaultDbProvider);
        ConfigureForDbProvider(provider, builder);
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
            CreateRespawnerOptions(DbAdapter.SqlServer),
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
            CreateRespawnerOptions(DbAdapter.Postgres),
            connectionString => new NpgsqlConnectionStringBuilder(connectionString) { IncludeErrorDetail = true }.ConnectionString);
    }

    private static RespawnerOptions CreateRespawnerOptions(IDbAdapter adapter) => new()
    {
        DbAdapter = adapter,
        SchemasToExclude = new[] { EfCoreUtils.MigrationsSchema }
    };
}
