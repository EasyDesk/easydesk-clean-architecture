using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Tests;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

[Collection(nameof(SampleApplicationTestCollection))]
public abstract class SampleIntegrationTest : IntegrationTest<SampleAppTestsFixture>
{
    protected SampleIntegrationTest(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    public SampleSeeder.Data TestData => Session.GetSeed<SampleSeeder.Data>();
}
