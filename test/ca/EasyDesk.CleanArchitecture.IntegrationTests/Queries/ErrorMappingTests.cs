using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Extensions;
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
        await Http
            .Get<Nothing>(TestErrorController.V10)
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldMapErrorForV_1_5()
    {
        await Http
            .Get<Nothing>(TestErrorController.V15)
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldMapUnversionedError()
    {
        await Http
            .Get<Nothing>(TestErrorController.Unversioned)
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldMapErrorForVersionV_0_1_ThatMissesMapper()
    {
        await Http
            .Get<Nothing>(TestErrorController.V01)
            .Send()
            .Verify();
    }

    [Fact]
    public async Task ShouldMapErrorForV_1_1_ThatMissesMapper()
    {
        await Http
            .Get<Nothing>(TestErrorController.V11)
            .Send()
            .Verify();
    }
}
