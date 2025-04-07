using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.Commons.Collections;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;
using EasyDesk.SampleApp.Application.V_1_0.OutgoingEvents;
using EasyDesk.SampleApp.Web;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Errors;

public class ErrorQueueTests : SampleAppIntegrationTest
{
    public ErrorQueueTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    private async Task ExpectMessageOnErrorQueue<T>(T message) where T : IIncomingCommand
    {
        await Session.DefaultBusEndpoint.Send(message);
        await WaitForBackoff();
        await Session.ErrorBusEndpoint.WaitForMessageOrFail(message);
    }

    private async Task WaitForBackoff()
    {
        var delays = EnumerableUtils
            .Iterate(0, x => x + 1)
            .Select(i => Retries.BackoffStrategy(i))
            .TakeWhile(x => x.IsPresent)
            .Select(x => x.Value);

        foreach (var delay in delays)
        {
            await Task.Delay(3000);
            Session.Clock.Advance(delay);
        }
    }

    [Fact]
    public async Task ShouldSendAMessageToTheErrorQueue_AfterException()
    {
        await ExpectMessageOnErrorQueue(new GenerateError());
    }

    [Fact]
    public async Task ShouldSendAMessageToTheErrorQueue_AfterError()
    {
        await ExpectMessageOnErrorQueue(new GenerateError2());
    }

    [Fact]
    public async Task ShouldSendAMessageToTheErrorQueue_AfterNotImplementedException_WithFailFast()
    {
        await ExpectMessageOnErrorQueue(new GenerateError3());
    }

    [Fact]
    public async Task ShouldEmitAnEvent_AfterScheduledRetries()
    {
        await Session.DefaultBusEndpoint.Subscribe<Error4Handled>();

        await Session.DefaultBusEndpoint.Send(new GenerateError4());

        await WaitForBackoff();

        await Session.DefaultBusEndpoint.WaitForMessageOrFail<Error4Handled>();
    }
}
