using EasyDesk.CleanArchitecture.Testing.Integration;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

[UsesVerify]
[Collection(nameof(SharedSampleApplicationFactory))]
public abstract class SampleIntegrationTest :
    AbstractIntegrationTest<SampleApplicationFactory, PersonController>
{
    protected SampleIntegrationTest(SampleApplicationFactory factory) : base(factory)
    {
    }

    protected override async Task OnDisposal() => await Factory.ResetDatabase();
}
