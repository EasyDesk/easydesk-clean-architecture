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
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;

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
        var container = new MsSqlBuilder()
            .WithUniqueName("sample-app-tests-sqlserver")
            .WithPassword("sample.123")
            .Build();

        builder
            .AddResettableSqlDatabase(
                container,
                CreateRespawnerOptions(DbAdapter.SqlServer))
            .WithConfiguration(x => x.Add(
                "ConnectionStrings:SqlServer",
                new SqlConnectionStringBuilder(container.GetConnectionString()) { TrustServerCertificate = true }.ConnectionString));
    }

    private static void ConfigurePostgreSql(WebServiceTestsFixtureBuilder builder)
    {
        var container = new PostgreSqlBuilder()
            .WithUniqueName("sample-app-tests-postgres")
            .WithDatabase("TestSampleDb")
            .WithUsername("sample")
            .WithPassword("sample")
            .Build();

        builder
            .AddResettableSqlDatabase(
                container,
                CreateRespawnerOptions(DbAdapter.Postgres))
            .WithConfiguration(x => x.Add(
                "ConnectionStrings:PostgreSql",
                new NpgsqlConnectionStringBuilder(container.GetConnectionString()) { IncludeErrorDetail = true }.ConnectionString));
    }

    private static RespawnerOptions CreateRespawnerOptions(IDbAdapter adapter) => new()
    {
        DbAdapter = adapter,
        SchemasToExclude = new[] { EfCoreUtils.MigrationsSchema }
    };
}
