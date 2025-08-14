using Autofac;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;
using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;
using EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.Postgres;
using EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.Sqlite;
using EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.SqlServer;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;
using EasyDesk.CleanArchitecture.Testing.Integration.Host;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Seeding;
using EasyDesk.CleanArchitecture.Testing.Integration.Time;
using EasyDesk.Commons.Utils;
using EasyDesk.SampleApp.Web;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using Microsoft.IdentityModel.Protocols.Configuration;
using NodaTime;
using Npgsql;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

public class SampleAppTestsFixture : IntegrationTestsFixture
{
    public const DbProvider DefaultDbProvider = DbProvider.Sqlite;

    private static readonly Dictionary<DbProvider, Action<TestFixtureConfigurer>> _providerConfigs = new()
    {
        [DbProvider.SqlServer] = ConfigureSqlServer,
        [DbProvider.PostgreSql] = ConfigurePostgreSql,
        [DbProvider.Sqlite] = ConfigureSqlite,
    };

    public DbProvider DbProvider { get; private set; }

    protected override void ConfigureFixture(TestFixtureConfigurer configurer)
    {
        configurer.AddFakeClock(Instant.FromUtc(2021, 11, 20, 11, 45));
        configurer.AddHttpTestHelper();
        configurer.AddMultitenancy();

        ConfigureDatabase(configurer);

        configurer.RegisterWebHost<PersonController>();

        configurer.AddInMemoryRebus();
        configurer.AddInMemoryRebusScheduler(Scheduler.Address, Duration.FromSeconds(1));

        configurer.RegisterLifetimeHooks(
            onInitialization: () =>
            {
                using var scope = Container.Resolve<ITestHost>().LifetimeScope.BeginUseCaseLifetimeScope();
                var migrationService = scope.Resolve<MigrationsService>();
                migrationService.MigrateSync();
            });

        configurer.AddSeedingOnInitialization(new SampleSeeder());
        configurer.AddDatabaseResetUsingTableCopies();
    }

    private void ConfigureDatabase(TestFixtureConfigurer configurer)
    {
        DbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER")
            .AsOption()
            .Map(p => Enums.ParseOption<DbProvider>(p).OrElseThrow(() => new InvalidConfigurationException("Invalid DB provider")))
            .OrElse(DefaultDbProvider);

        _providerConfigs[DbProvider](configurer);

        configurer.ConfigureHost(h => h.WithConfiguration("DbProvider", DbProvider.ToString()));
    }

    private static void ConfigureSqlServer(TestFixtureConfigurer configurer)
    {
        var container = new MsSqlBuilder()
            .WithUniqueName("sample-app-tests-sqlserver")
            .WithPassword("sample.123")
            .Build();

        configurer.AddSqlServerDatabase(container, "SampleDb", x => x
            .OverrideConnectionStringFromConfiguration("ConnectionStrings:SqlServer"));
    }

    private static void ConfigureSqlite(TestFixtureConfigurer configurer)
    {
        configurer.AddSqliteDatabase("ConnectionStrings:Sqlite", "db-test.sqlite");
    }

    private static void ConfigurePostgreSql(TestFixtureConfigurer configurer)
    {
        var container = new PostgreSqlBuilder()
            .WithUniqueName("sample-app-tests-postgres")
            .WithDatabase("TestSampleDb")
            .WithUsername("sample")
            .WithPassword("sample")
            .Build();

        configurer.AddPostgresDatabase(container, x => x
            .ModifyConnectionString(c => new NpgsqlConnectionStringBuilder(c) { IncludeErrorDetail = true, }.ConnectionString)
            .OverrideConnectionStringFromConfiguration("ConnectionStrings:PostgreSql"));
    }
}
