using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Seeding;

public static class FixtureSeedingExtensions
{
    public static WebServiceTestsFixtureBuilder SeedOnInitialization<T>(
        this WebServiceTestsFixtureBuilder builder,
        Func<ISeeder<T>> seederFactory,
        Action<T>? onSeedingCompleted = null)
    {
        return builder.OnInitialization(_ => Seed(seederFactory, onSeedingCompleted));
    }

    public static WebServiceTestsFixtureBuilder SeedBeforeEachTest<T>(
        this WebServiceTestsFixtureBuilder builder,
        Func<ISeeder<T>> seederFactory,
        Action<T>? onSeedingCompleted = null)
    {
        return builder.BeforeEachTest(_ => Seed(seederFactory, onSeedingCompleted));
    }

    private static async Task Seed<T>(Func<ISeeder<T>> seederFactory, Action<T>? onSeedingCompleted)
    {
        await using var seeder = seederFactory();
        var result = await seeder.Seed();
        onSeedingCompleted?.Invoke(result);
    }
}
