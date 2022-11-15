using EasyDesk.CleanArchitecture.Testing.Integration;
using EasyDesk.SampleApp.Web.Controllers.V_1_0;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

[UsesVerify]
public abstract class SampleIntegrationTest : AbstractIntegrationTest<SampleApplicationFactory, PersonController>
{
    protected SampleIntegrationTest(SampleApplicationFactory factory) : base(factory)
    {
    }

    protected override async Task OnDisposal() => await Factory.ResetDatabase();
}
