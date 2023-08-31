using EasyDesk.SampleApp.Web.Controllers.V_1_0.Test;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Queries;

public class ErrorMappingTests : SampleIntegrationTest
{
    public ErrorMappingTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ShouldMapErrorForV_1_0()
    {
        var response = await Http
            .Get<Nothing>(TestController.TestDomainError)
            .WithApiVersion(new(1, 0))
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldMapErrorForV_1_1()
    {
        var response = await Http
            .Get<Nothing>(TestController.TestDomainError)
            .WithApiVersion(new(1, 1))
            .Send()
            .AsVerifiable();

        await Verify(response);
    }
}
