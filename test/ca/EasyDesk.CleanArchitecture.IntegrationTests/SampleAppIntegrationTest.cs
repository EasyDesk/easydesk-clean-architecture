using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration.Seeding;
using EasyDesk.CleanArchitecture.Testing.Integration.Tests;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

[Collection(nameof(SampleAppTestCollection))]
public abstract class SampleAppIntegrationTest : IntegrationTest<SampleAppTestsFixture>
{
    protected SampleAppIntegrationTest(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    public SampleSeeder.Data TestData => Fixture.GetSeed<SampleSeeder.Data>();
}
