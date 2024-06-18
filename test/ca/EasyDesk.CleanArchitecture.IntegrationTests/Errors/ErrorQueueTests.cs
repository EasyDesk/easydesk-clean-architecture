using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.Commons.Collections;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;
using EasyDesk.SampleApp.Web;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Errors;

public class ErrorQueueTests : SampleIntegrationTest
{
    public ErrorQueueTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    private async Task RunTest<T>(T message) where T : IIncomingCommand
    {
        await DefaultBusEndpoint.Send(message);

        var delays = EnumerableUtils
            .Iterate(0, x => x + 1)
            .Select(i => Retries.BackoffStrategy(i))
            .TakeWhile(x => x.IsPresent)
            .Select(x => x.Value);

        foreach (var delay in delays)
        {
            await Task.Delay(2000);
            Clock.Advance(delay);
        }

        await ErrorBusEndpoint.WaitForMessageOrFail(message);
    }

    [Fact]
    public async Task ShouldSendAMessageToTheErrorQueue_AfterException()
    {
        await RunTest(new GenerateError());
    }

    [Fact]
    public async Task ShouldSendAMessageToTheErrorQueue_AfterError()
    {
        await RunTest(new GenerateError2());
    }

    [Fact]
    public async Task ShouldSendAMessageToTheErrorQueue_AfterNotImplementedException_WithFailFast()
    {
        await RunTest(new GenerateError3());
    }
}
