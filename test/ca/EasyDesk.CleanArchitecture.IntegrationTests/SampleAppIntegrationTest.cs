using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Tests;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

[Collection(nameof(SampleAppTestCollection))]
public abstract class SampleAppIntegrationTest : IntegrationTest<SampleAppTestsFixture>
{
    protected SampleAppIntegrationTest(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    public SampleSeeder.Data TestData => Session.GetSeed<SampleSeeder.Data>();
}
