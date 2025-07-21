using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Extensions;
using EasyDesk.SampleApp.Application.V_1_0.OutgoingEvents;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Cancellation;
using NodaTime;
using Shouldly;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Cancellation;

public class CancellationTests : SampleAppIntegrationTest
{
    public CancellationTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ShouldCancelTheRequestBeforeCompletion_WhenCancellationIsRequested()
    {
        var waitTime = Duration.FromSeconds(5);
        var timeout = Duration.FromSeconds(1);

        await Session.DefaultBusEndpoint.Subscribe<CancellationFailed>();

        await Should.ThrowAsync<TaskCanceledException>(async () =>
        {
            await Session.Http
                .Post<Nothing, Nothing>(CancellationRoutes.CancellableRequest, Nothing.Value)
                .With(x => x
                    .Query("waitTime", waitTime.ToString())
                    .Timeout(timeout))
                .Send()
                .EnsureSuccess();
        });

        await Session.DefaultBusEndpoint.FailIfMessageIsReceived<CancellationFailed>(waitTime + Duration.FromSeconds(10));
    }

    [Fact]
    public async Task ShouldNotCancelTheRequestBeforeCompletion_IfCancellationIsNotRequested()
    {
        var waitTime = Duration.FromSeconds(1);
        var timeout = Duration.FromSeconds(15);
        await Session.DefaultBusEndpoint.Subscribe<CancellationFailed>();

        await Session.Http
            .Post<Nothing, Nothing>(CancellationRoutes.CancellableRequest, Nothing.Value)
            .With(x => x
                .Query("waitTime", waitTime.ToString())
                .Timeout(timeout))
            .Send()
            .Verify();

        await Session.DefaultBusEndpoint.WaitForMessageOrFail<CancellationFailed>();
    }
}
