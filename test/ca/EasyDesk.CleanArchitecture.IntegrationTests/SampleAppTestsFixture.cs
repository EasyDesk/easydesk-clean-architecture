using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;
using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;
using EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.Postgres;
using EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.SqlServer;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.CleanArchitecture.Testing.Integration.Seeding;
using EasyDesk.Commons.Utils;
using EasyDesk.SampleApp.Web;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using Npgsql;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

public class SampleAppTestsFixture : WebServiceTestsFixture
{
    public const DbProvider DefaultDbProvider = DbProvider.SqlServer;

    private static readonly Dictionary<DbProvider, Action<WebServiceTestsFixtureBuilder>> _providerConfigs = new()
    {
        { DbProvider.SqlServer, ConfigureSqlServer },
        { DbProvider.PostgreSql, ConfigurePostgreSql },
    };

    public SampleAppTestsFixture() : base(typeof(PersonController))
    {
    }

    public SampleTestData TestData { get; private set; } = default!;

    protected override void ConfigureFixture(WebServiceTestsFixtureBuilder builder)
    {
        var provider = Environment.GetEnvironmentVariable("DB_PROVIDER")
            .AsOption()
            .Map(p => Enums.ParseOption<DbProvider>(p).OrElseThrow(() => new Exception("Invalid DB provider")))
            .OrElse(DefaultDbProvider);
        ConfigureForDbProvider(provider, builder);

        builder
            .SeedBeforeEachTest(() => new SampleSeeder(this), onSeedingCompleted: x => TestData = x)
            .AddInMemoryRebus();
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
            .AddSqlServerDatabase(container, "SampleDb")
            .WithRespawn(ConfigureRespawnerOptions)
            .OverrideConnectionStringFromConfiguration("ConnectionStrings:SqlServer");
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
            .AddPostgresDatabase(container)
            .WithRespawn(ConfigureRespawnerOptions)
            .ModifyConnectionString(c => new NpgsqlConnectionStringBuilder(c) { IncludeErrorDetail = true }.ConnectionString)
            .OverrideConnectionStringFromConfiguration("ConnectionStrings:PostgreSql");
    }

    private static void ConfigureRespawnerOptions(RespawnerOptionsBuilder respawnerOptions) =>
        respawnerOptions.ExcludeSchemas(EfCoreUtils.MigrationsSchema);
}
