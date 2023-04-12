using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.SampleApp.Application.IncomingCommands;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Errors;

public class ErrorQueueTests : SampleIntegrationTest
{
    public ErrorQueueTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ShouldSendAMessageToTheErrorQueue()
    {
        var message = new GenerateError();
        var bus = NewBus(WebService.Services.GetRequiredService<RebusMessagingOptions>().ErrorQueueName);

        await bus.Send(message);

        await bus.WaitForMessageOrFail(message);
    }
}
