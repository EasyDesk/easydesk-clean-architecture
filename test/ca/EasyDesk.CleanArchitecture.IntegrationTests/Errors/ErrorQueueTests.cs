using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;
using Microsoft.Extensions.DependencyInjection;

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
        var bus = NewBus(WebService.Services.GetRequiredService<RebusMessagingOptions>().ErrorQueueName);

        await bus.Send(message);

        await bus.WaitForMessageOrFail(message);
    }

    [Fact]
    public async Task ShouldSendAMessageToTheErrorQueue_AfterError()
    {
        var message = new GenerateError2();
        var bus = NewBus(WebService.Services.GetRequiredService<RebusMessagingOptions>().ErrorQueueName);

        await bus.Send(message);

        await bus.WaitForMessageOrFail(message);
    }

    [Fact]
    public async Task ShouldSendAMessageToTheErrorQueue_AfterNotImplementedException_WithFailFast()
    {
        var message = new GenerateError3();
        var bus = NewBus(WebService.Services.GetRequiredService<RebusMessagingOptions>().ErrorQueueName);

        await bus.Send(message);

        await bus.WaitForMessageOrFail(message);
    }
}
