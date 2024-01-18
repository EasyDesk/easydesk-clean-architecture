using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

[Collection(nameof(SampleApplicationTestCollection))]
public abstract class SampleIntegrationTest : WebServiceIntegrationTest<SampleAppTestsFixture>
{
    protected SampleIntegrationTest(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    public SampleSeeder.Data TestData => Fixture.Seed;
}
