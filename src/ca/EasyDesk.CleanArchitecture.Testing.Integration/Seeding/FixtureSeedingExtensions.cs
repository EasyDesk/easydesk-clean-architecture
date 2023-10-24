using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Seeding;

public static class FixtureSeedingExtensions
{
    public static WebServiceTestsFixtureBuilder SeedOnInitialization<T>(
        this WebServiceTestsFixtureBuilder builder,
        Func<ISeeder<T>> seederFactory,
        T data)
    {
        return builder.OnInitialization(_ => Seed(seederFactory, data));
    }

    public static WebServiceTestsFixtureBuilder SeedBeforeEachTest<T>(
        this WebServiceTestsFixtureBuilder builder,
        Func<ISeeder<T>> seederFactory,
        T data)
    {
        return builder.BeforeEachTest(_ => Seed(seederFactory, data));
    }

    private static async Task Seed<T>(Func<ISeeder<T>> seederFactory, T data)
    {
        await using var seeder = seederFactory();
        await seeder.Seed(data);
    }
}
