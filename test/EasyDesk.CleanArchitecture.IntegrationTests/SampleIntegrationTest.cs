using EasyDesk.CleanArchitecture.Testing.Integration;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

[UsesVerify]
[Collection(nameof(SharedSampleApplicationFixture))]
public abstract class SampleIntegrationTest : WebServiceIntegrationTest<SampleAppTestsFixture>
{
    protected SampleIntegrationTest(SampleAppTestsFixture factory) : base(factory)
    {
    }
}
