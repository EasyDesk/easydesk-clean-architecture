using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Seeding;

public static class FixtureSeedingExtensions
{
    public static WebServiceTestsFixtureBuilder<T> SeedOnInitialization<T>(
        this WebServiceTestsFixtureBuilder<T> builder,
        Func<T, WebServiceFixtureSeeder<T>> seederFactory) where T : WebServiceTestsFixture<T>
    {
        return builder.OnInitialization(Seed(seederFactory));
    }

    public static WebServiceTestsFixtureBuilder<T> SeedBeforeEachTest<T>(
        this WebServiceTestsFixtureBuilder<T> builder,
        Func<T, WebServiceFixtureSeeder<T>> seederFactory) where T : WebServiceTestsFixture<T>
    {
        return builder.BeforeEachTest(Seed(seederFactory));
    }

    private static AsyncAction<T> Seed<T>(Func<T, WebServiceFixtureSeeder<T>> seederFactory)
         where T : WebServiceTestsFixture<T>
    {
        return async fixture =>
        {
            await using var seeder = seederFactory(fixture);
            await seeder.Seed();
        };
    }
}
