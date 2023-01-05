using EasyDesk.CleanArchitecture.Testing.Integration;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

[UsesVerify]
[Collection(nameof(SampleApplicationTestCollection))]
public abstract class SampleIntegrationTest : WebServiceIntegrationTest<SampleAppTestsFixture>
{
    protected SampleIntegrationTest(SampleAppTestsFixture factory) : base(factory)
    {
    }
}
