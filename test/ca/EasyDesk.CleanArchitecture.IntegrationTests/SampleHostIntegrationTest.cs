using EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Tests;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

[Collection(nameof(SampleHostTestCollection))]
public abstract class SampleHostIntegrationTest : IntegrationTest<SampleHostTestsFixture>
{
    protected SampleHostIntegrationTest(SampleHostTestsFixture fixture) : base(fixture)
    {
    }
}
