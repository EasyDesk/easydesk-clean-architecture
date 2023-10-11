using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Seeding;

public static class FixtureSeedingExtensions
{
    public static WebServiceTestsFixtureBuilder SeedOnInitialization(this WebServiceTestsFixtureBuilder builder, ISeeder seeder) =>
        builder.OnInitialization(_ => seeder.Seed());

    public static WebServiceTestsFixtureBuilder SeedBeforeEachTest(this WebServiceTestsFixtureBuilder builder, ISeeder seeder) =>
        builder.BeforeEachTest(_ => seeder.Seed());
}
