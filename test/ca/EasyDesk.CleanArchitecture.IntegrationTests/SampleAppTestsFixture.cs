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
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Npgsql;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

public class SampleAppTestsFixture : WebServiceTestsFixture<SampleAppTestsFixture>
    .WithSeeding<SampleSeeder.Data>
    .FollowingFixtureLifetime
{
    public const DbProvider DefaultDbProvider = DbProvider.PostgreSql;

    private static readonly Dictionary<DbProvider, Action<WebServiceTestsFixtureBuilder<SampleAppTestsFixture>>> _providerConfigs = new()
    {
        { DbProvider.SqlServer, ConfigureSqlServer },
        { DbProvider.PostgreSql, ConfigurePostgreSql },
    };

    public SampleAppTestsFixture() : base(typeof(PersonController))
    {
    }

    protected override Instant InitialInstant => Instant.FromUtc(2021, 11, 20, 11, 45);

    protected override void ConfigureFixture(WebServiceTestsFixtureBuilder<SampleAppTestsFixture> builder)
    {
        ConfigureDbProvider(builder);

        builder.AddInMemoryRebus();
        builder.AddInMemoryRebusScheduler(Scheduler.Address, Duration.FromSeconds(1));
    }

    private void ConfigureDbProvider(WebServiceTestsFixtureBuilder<SampleAppTestsFixture> builder)
    {
        var provider = Environment.GetEnvironmentVariable("DB_PROVIDER")
            .AsOption()
            .Map(p => Enums.ParseOption<DbProvider>(p).OrElseThrow(() => new Exception("Invalid DB provider")))
            .OrElse(DefaultDbProvider);

        builder.WithConfiguration("DbProvider", provider.ToString());
        _providerConfigs[provider](builder);

        builder.ConfigureWebService(web => web.BeforeStart(host =>
        {
            using var scope = host.Services.CreateScope();
            var migrationService = scope.ServiceProvider.GetRequiredService<MigrationsService>();
            migrationService.MigrateSync();
        }));
    }

    protected override WebServiceFixtureSeeder<SampleAppTestsFixture, SampleSeeder.Data> CreateSeeder(SampleAppTestsFixture fixture) =>
        new SampleSeeder(fixture);

    private static ISqlDatabaseFixtureBuilder ConfigureDatabaseDefaults(ISqlDatabaseFixtureBuilder builder) => builder
        .WithTableCopies();

    private static void ConfigureSqlServer(WebServiceTestsFixtureBuilder<SampleAppTestsFixture> builder)
    {
        var container = new MsSqlBuilder()
            .WithUniqueName("sample-app-tests-sqlserver")
            .WithPassword("sample.123")
            .Build();

        ConfigureDatabaseDefaults(builder.AddSqlServerDatabase(container, "SampleDb"))
            .OverrideConnectionStringFromConfiguration("ConnectionStrings:SqlServer");
    }

    private static void ConfigurePostgreSql(WebServiceTestsFixtureBuilder<SampleAppTestsFixture> builder)
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
}
