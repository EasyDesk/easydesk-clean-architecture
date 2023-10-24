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
    public const DbProvider DefaultDbProvider = DbProvider.PostgreSql;

    private static readonly Dictionary<DbProvider, Action<WebServiceTestsFixtureBuilder>> _providerConfigs = new()
    {
        { DbProvider.SqlServer, ConfigureSqlServer },
        { DbProvider.PostgreSql, ConfigurePostgreSql },
    };

    public SampleAppTestsFixture() : base(typeof(PersonController))
    {
    }

    public SampleTestData TestData { get; } = new();

    protected override void ConfigureFixture(WebServiceTestsFixtureBuilder builder)
    {
        var provider = Environment.GetEnvironmentVariable("DB_PROVIDER")
            .AsOption()
            .Map(p => Enums.ParseOption<DbProvider>(p).OrElseThrow(() => new Exception("Invalid DB provider")))
            .OrElse(DefaultDbProvider);

        builder
            .SeedOnInitialization(() => new SampleSeeder(this), TestData)
            .AddInMemoryRebus();
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

        ConfigureDatabaseDefaults(builder.AddSqlServerDatabase(container, "SampleDb"))
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

        ConfigureDatabaseDefaults(builder.AddPostgresDatabase(container))
            .ModifyConnectionString(c => new NpgsqlConnectionStringBuilder(c) { IncludeErrorDetail = true }.ConnectionString)
            .OverrideConnectionStringFromConfiguration("ConnectionStrings:PostgreSql");
    }

    private static ISqlDatabaseFixtureBuilder ConfigureDatabaseDefaults(ISqlDatabaseFixtureBuilder builder) => builder
        ////.WithRespawn(x => x.ExcludeSchemas(EfCoreUtils.MigrationsSchema));
        .WithTableCopies();
}
