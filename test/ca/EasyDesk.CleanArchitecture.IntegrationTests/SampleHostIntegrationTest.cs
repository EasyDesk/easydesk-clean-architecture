using EasyDesk.CleanArchitecture.Testing.Integration.Tests;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

[Collection(typeof(SampleHostTestCollection))]
public abstract class SampleHostIntegrationTest : IntegrationTest<SampleHostTestsFixture>
{
    protected SampleHostIntegrationTest(SampleHostTestsFixture fixture) : base(fixture)
    {
    }
}
