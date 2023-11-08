using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Extensions;
using EasyDesk.SampleApp.Application.V_1_0.OutgoingEvents;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Cancellation;
using NodaTime;
using Shouldly;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Cancellation;

public class CancellationTests : SampleIntegrationTest
{
    public CancellationTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ShouldCancelTheRequestBeforeCompletion_WhenCancellationIsRequested()
    {
        var waitTime = Duration.FromSeconds(5);
        var timeout = Duration.FromSeconds(1);

        await DefaultBusEndpoint.Subscribe<CancellationFailed>();

        await Should.ThrowAsync<TaskCanceledException>(async () =>
        {
            await Http
                .Post<Nothing, Nothing>(CancellationRoutes.CancellableRequest, Nothing.Value)
                .WithQuery("waitTime", waitTime.ToString())
                .WithTimeout(timeout)
                .Send()
                .EnsureSuccess();
        });

        await DefaultBusEndpoint.FailIfMessageIsReceived<CancellationFailed>(waitTime + Duration.FromSeconds(10));
    }

    [Fact]
    public async Task ShouldNotCancelTheRequestBeforeCompletion_IfCancellationIsNotRequested()
    {
        var waitTime = Duration.FromSeconds(1);
        var timeout = Duration.FromSeconds(15);
        await DefaultBusEndpoint.Subscribe<CancellationFailed>();

        await Http
            .Post<Nothing, Nothing>(CancellationRoutes.CancellableRequest, Nothing.Value)
            .WithQuery("waitTime", waitTime.ToString())
            .WithTimeout(timeout)
            .Send()
            .Verify();

        await DefaultBusEndpoint.WaitForMessageOrFail<CancellationFailed>();
    }
}
