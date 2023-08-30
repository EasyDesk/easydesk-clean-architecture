using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Errors;

public class ErrorQueueTests : SampleIntegrationTest
{
    public ErrorQueueTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ShouldSendAMessageToTheErrorQueue_AfterException()
    {
        var message = new GenerateError();

        await DefaultBusEndpoint.Send(message);

        await ErrorBusEndpoint.WaitForMessageOrFail(message);
    }

    [Fact]
    public async Task ShouldSendAMessageToTheErrorQueue_AfterError()
    {
        var message = new GenerateError2();

        await DefaultBusEndpoint.Send(message);

        await ErrorBusEndpoint.WaitForMessageOrFail(message);
    }

    [Fact]
    public async Task ShouldSendAMessageToTheErrorQueue_AfterNotImplementedException_WithFailFast()
    {
        var message = new GenerateError3();

        await DefaultBusEndpoint.Send(message);

        await ErrorBusEndpoint.WaitForMessageOrFail(message);
    }
}
