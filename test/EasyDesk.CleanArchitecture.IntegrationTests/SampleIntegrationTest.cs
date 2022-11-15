using EasyDesk.CleanArchitecture.Testing.Integration;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

[UsesVerify]
public abstract class SampleIntegrationTest : AbstractIntegrationTest<SampleApplicationFactory, PersonController>, IClassFixture<SampleApplicationFactory>
{
    protected SampleIntegrationTest(SampleApplicationFactory factory) : base(factory)
    {
    }

    protected override async Task OnDisposal() => await Factory.ResetDatabase();
}
