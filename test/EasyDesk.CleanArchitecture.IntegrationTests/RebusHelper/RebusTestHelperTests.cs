using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Testing.Integration.Rebus;
using NodaTime;
using Rebus.Config;
using Rebus.Routing.TypeBased;

namespace EasyDesk.CleanArchitecture.IntegrationTests.RebusHelper;

public class RabbitMqContainerFixture : IAsyncLifetime
{
    public RabbitMqTestcontainer RabbitMq { get; } = new TestcontainersBuilder<RabbitMqTestcontainer>()
        .WithMessageBroker(new RabbitMqTestcontainerConfiguration
        {
            Username = "admin",
            Password = "admin",
        })
        .Build();

    public async Task InitializeAsync()
    {
        await RabbitMq.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await RabbitMq.DisposeAsync();
    }
}

public class RebusTestHelperTests : IClassFixture<RabbitMqContainerFixture>, IAsyncLifetime
{
    private record Command(int Value) : ICommand;

    private record Event(int Value) : IEvent;

    private const string SenderAddress = "sender";
    private const string ReceiverAddress = "receiver";

    private static readonly Duration _defaultTimeout = Duration.FromSeconds(5);
    private static readonly Duration _computationSlack = Duration.FromMilliseconds(500);

    private readonly string _rabbitMqConnection;
    private readonly RebusTestHelper _sender;
    private readonly RebusTestHelper _receiver;

    public RebusTestHelperTests(RabbitMqContainerFixture rabbitMqContainerFixture)
    {
        _rabbitMqConnection = rabbitMqContainerFixture.RabbitMq.ConnectionString;
        _sender = CreateBus(SenderAddress);
        _receiver = CreateBus(ReceiverAddress);
    }

    private RebusTestHelper CreateBus(string endpoint)
    {
        return new RebusTestHelper(rebus => rebus
            .Transport(t => t.UseRabbitMq(_rabbitMqConnection, endpoint))
            .Routing(r => r.TypeBased().MapFallback(ReceiverAddress)));
    }

    [Fact]
    public async Task SentCommandsShouldBeReceivedImmediately()
    {
        var command = new Command(1);

        await _sender.Send(command);

        await _receiver.WaitForMessageOrFail(command, _defaultTimeout);
    }

    [Fact]
    public async Task PublishedEventsShouldBeReceivedImmediately()
    {
        var ev = new Event(1);
        await _receiver.Subscribe<Event>();

        await _sender.Publish(ev);

        await _receiver.WaitForMessageOrFail(ev, _defaultTimeout);
    }

    [Fact]
    public async Task PublishedEventsShouldNotBeReceivedIfReceiverDidntSubscribe()
    {
        var ev = new Event(1);

        await _sender.Publish(ev);

        await _receiver.FailIfMessageIsReceivedWithin(ev, _defaultTimeout);
    }

    [Fact]
    public async Task SentCommandsNotMatchingThePredicateShouldNotBeReceived()
    {
        var command = new Command(1);

        await _sender.Send(command);

        await _receiver.FailIfMessageIsReceivedWithin<Command>(cmd => cmd.Value != 1, _defaultTimeout);
    }

    [Fact]
    public async Task SentCommandsShouldNotBeStoredInTheQueueIfTheyAreImmediatelyHandled()
    {
        var command = new Command(1);

        await _sender.Send(command);

        await _receiver.WaitForMessageOrFail(command, _defaultTimeout);
        await _receiver.FailIfMessageIsReceivedWithin(command, Duration.Zero);
    }

    [Fact]
    public async Task SentCommandsShouldBeRemovedFromTheQueueWhenHandled()
    {
        var command = new Command(1);

        await _sender.Send(command);

        await _receiver.FailIfMessageIsReceivedWithin<Command>(_ => false, _defaultTimeout);
        await _receiver.WaitForMessageOrFail(command, Duration.Zero);
        await _receiver.FailIfMessageIsReceivedWithin(command, Duration.Zero);
    }

    [Fact]
    public async Task DeferredCommandsShouldBeReceivedAfterTheGivenDelay()
    {
        var command = new Command(1);
        var delay = Duration.FromSeconds(5);

        Defer(command, delay);

        await _receiver.WaitForMessageAfterDelayOrFail(command, delay, _defaultTimeout);
    }

    private async void Defer<T>(T command, Duration delay, Duration? tolerance = null) where T : ICommand
    {
        await Task.Run(async () =>
        {
            await Task.Delay((delay + (tolerance ?? _computationSlack)).ToTimeSpan());
            await _sender.Send(command);
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _receiver.DisposeAsync();
    }
}
