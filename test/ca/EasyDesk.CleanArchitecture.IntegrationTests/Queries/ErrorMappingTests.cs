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
            .Get<Nothing>(TestErrorController.V10)
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldMapErrorForV_1_1()
    {
        var response = await Http
            .Get<Nothing>(TestErrorController.V11)
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task ShouldMapUnversionedError()
    {
        var response = await Http
            .Get<Nothing>(TestErrorController.Unversioned)
            .Send()
            .AsVerifiable();

        await Verify(response);
    }
}
