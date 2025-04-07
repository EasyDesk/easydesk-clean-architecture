using EasyDesk.CleanArchitecture.Testing.Integration.Bus;
using EasyDesk.CleanArchitecture.Testing.Integration.Lifetime;
using EasyDesk.Commons.Tasks;
using EasyDesk.SampleHostApp.Service.V_1_0.AsyncCommands;
using NodaTime;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Testing;

internal class HostIntegrationTestingFrameworkTests : SampleHostIntegrationTest
{
    private const string MessageText = "Hello World!";

    public HostIntegrationTestingFrameworkTests(SampleHostTestsFixture factory) : base(factory)
    {
        Recipient = Session.NewBusEndpoint(ReceiveMessage.RecipientQueue);
    }

    private ITestBusEndpoint Recipient { get; }

    public async Task TestSendMessage()
    {
        await Recipient.Send(new SendMessage
        {
            Message = MessageText,
        });

        await Recipient.WaitForMessageOrFail(new ReceiveMessage
        {
            Message = MessageText,
        });
    }
}

public class HostIntegrationTestTestFixtureLifecycleTests
{
    private async Task UsingFixture(AsyncAction<SampleHostTestsFixture> action) =>
        await AsyncLifetime.UsingAsyncLifetime(() => new SampleHostTestsFixture(), action);

    private async Task RunTest(SampleHostTestsFixture fixture)
    {
        await AsyncLifetime.UsingAsyncLifetime(() => new HostIntegrationTestingFrameworkTests(fixture), async integrationTest =>
        {
            await integrationTest.TestSendMessage();
        });
    }

    [Fact]
    public async Task TestFixtureLifecycle()
    {
        await UsingFixture(async fixture =>
        {
            for (var i = 0; i < 5; i++)
            {
                await RunTest(fixture);
            }
        });
    }

    [Fact]
    public async Task TestFixtureLifecycle_SkippingWaits()
    {
        await UsingFixture(async fixture =>
        {
            for (var i = 0; i < 5; i++)
            {
                await RunTest(fixture);
            }
        });
    }

    [Fact]
    public async Task TestFixtureLifecycle_WithParallelism()
    {
        async Task Run(Duration delay)
        {
            await Task.Delay(delay.ToTimeSpan());
            await UsingFixture(async fixture =>
            {
                for (var i = 0; i < 5; i++)
                {
                    await RunTest(fixture);
                }
            });
        }
        await Task.WhenAll(
            Run(Duration.FromSeconds(1)),
            Run(Duration.FromSeconds(2)),
            Run(Duration.FromSeconds(5)),
            Run(Duration.FromSeconds(10)));
    }
}
