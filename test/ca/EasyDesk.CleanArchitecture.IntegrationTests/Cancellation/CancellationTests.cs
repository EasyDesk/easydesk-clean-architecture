﻿using EasyDesk.SampleApp.Application.OutgoingEvents;
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
        var bus = NewBus();
        await bus.Subscribe<CancellationFailed>();

        await Should.ThrowAsync<TaskCanceledException>(async () =>
        {
            await Http
                .Post<Nothing, Nothing>(CancellationRoutes.CancellableRequest, Nothing.Value)
                .WithQuery("waitTime", waitTime.ToString())
                .WithTimeout(timeout)
                .Send()
                .AsVerifiable();
        });

        await bus.FailIfMessageIsReceivedWithin<CancellationFailed>(waitTime + Duration.FromSeconds(10));
    }

    [Fact]
    public async Task ShouldNotCancelTheRequestBeforeCompletion_IfCancellationIsNotRequested()
    {
        var waitTime = Duration.FromSeconds(1);
        var timeout = Duration.FromSeconds(15);
        var bus = NewBus();
        await bus.Subscribe<CancellationFailed>();

        var response = await Http
            .Post<Nothing, Nothing>(CancellationRoutes.CancellableRequest, Nothing.Value)
            .WithQuery("waitTime", waitTime.ToString())
            .WithTimeout(timeout)
            .Send()
            .AsVerifiable();

        await bus.WaitForMessageOrFail<CancellationFailed>();

        await Verify(response);
    }
}