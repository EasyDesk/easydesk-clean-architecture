using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;
using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Multitenancy;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using EasyDesk.Testing.MatrixExpansion;
using NodaTime;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Shouldly;
using Testcontainers.RabbitMq;

namespace EasyDesk.CleanArchitecture.IntegrationTests.RebusHelper;

public class RabbitMqContainerFixture : IAsyncLifetime
{
    public RabbitMqContainer RabbitMq { get; } = new RabbitMqBuilder()
        .WithUniqueName("rebus-helper-tests-rabbitmq")
        .WithUsername("admin")
        .WithPassword("admin")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await RabbitMq.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await RabbitMq.DisposeAsync();
    }
}

public class RebusTestBusTests : IClassFixture<RabbitMqContainerFixture>, IAsyncLifetime
{
    private record Command(int Value) : ICommand;

    private record Event(int Value) : IEvent;

    private readonly string _senderAddress;
    private readonly string _receiverAddress;

    private static readonly Duration _defaultTimeout = Duration.FromSeconds(10);
    private static readonly Duration _computationSlack = Duration.FromMilliseconds(500);

    private readonly string _rabbitMqConnection;
    private readonly ITestBusEndpoint _sender;
    private readonly ITestBusEndpoint _receiver;

    private readonly TestTenantManager _tenantManager = new(new(None));

    public RebusTestBusTests(RabbitMqContainerFixture rabbitMqContainerFixture)
    {
        _rabbitMqConnection = rabbitMqContainerFixture.RabbitMq.GetConnectionString();
        _senderAddress = $"sender-{Guid.NewGuid()}";
        _receiverAddress = $"receiver-{Guid.NewGuid()}";
        _sender = CreateBus(_senderAddress);
        _receiver = CreateBus(_receiverAddress);
    }

    private RebusTestBusEndpoint CreateBus(string endpoint)
    {
        return new(
            rebus => rebus
                .Transport(t => t.UseRabbitMq(_rabbitMqConnection, endpoint))
                .Routing(r => r.TypeBased().MapFallback(_receiverAddress)),
            _tenantManager);
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

        await _receiver.FailIfMessageIsReceived(ev, _defaultTimeout);
    }

    [Fact]
    public async Task SentCommandsNotMatchingThePredicateShouldNotBeReceived()
    {
        var command = new Command(1);

        await _sender.Send(command);

        await _receiver.FailIfMessageIsReceived<Command>(cmd => cmd.Value != 1, _defaultTimeout);
    }

    [Fact]
    public async Task SentCommandsShouldNotBeStoredInTheQueueIfTheyAreImmediatelyHandled()
    {
        var command = new Command(1);

        await _sender.Send(command);

        await _receiver.WaitForMessageOrFail(command, _defaultTimeout);
        await _receiver.FailIfMessageIsReceived(command, Duration.Zero);
    }

    [Fact]
    public async Task SentCommandsShouldBeRemovedFromTheQueueWhenHandled()
    {
        var command = new Command(1);

        await _sender.Send(command);

        await _receiver.FailIfMessageIsReceived<Command>(_ => false, _defaultTimeout);
        await _receiver.WaitForMessageOrFail(command, Duration.Zero);
        await _receiver.FailIfMessageIsReceived(command, Duration.Zero);
    }

    [Fact]
    public async Task DeferredCommandsShouldBeReceivedAfterTheGivenDelay()
    {
        var command = new Command(1);
        var delay = Duration.FromSeconds(5);

        Defer(command, delay);

        await _receiver.WaitForMessageAfterDelayOrFail(command, delay, _defaultTimeout);
    }

    [Fact]
    public async Task MultipleCommandsShouldBeReceivedUntilQuiet()
    {
        var commands = new Command[] { new(1), new(2), new(3), };

        await Task.WhenAll(commands.Select(e => _sender.Send(e)));

        var received = await _receiver.WaitForMessagesUntilQuiet<Command>(_defaultTimeout).ToEnumerableAsync();
        received.ShouldBe(commands, ignoreOrder: true);
    }

    public static IEnumerable<object?[]> TenantActions()
    {
        var axis = new TenantInfo[] { TenantInfo.Tenant(new TenantId("a")), TenantInfo.Tenant(new TenantId("b")), TenantInfo.Public, };
        return Matrix.Builder()
            .OptionAxis<TenantInfo>(axis)
            .OptionAxis<TenantInfo>(axis)
            .Build();
    }

    [Theory]
    [MemberData(nameof(TenantActions))]
    public async Task ShouldFilterCommandInTenant(Option<TenantInfo> before, Option<TenantInfo> after)
    {
        var command = new Command(7);
        using var scopeBefore = _tenantManager.MoveTo(before);
        await _sender.Send(command);

        using var scopeAfter = _tenantManager.MoveTo(after);
        if (after.IsAbsent || before.OrElse(TenantInfo.Public) == after.OrElse(TenantInfo.Public))
        {
            await _receiver.WaitForMessageOrFail(command, _defaultTimeout);
        }
        else
        {
            await _receiver.FailIfMessageIsReceived(command, _defaultTimeout);
        }
    }

    [Theory]
    [MemberData(nameof(TenantActions))]
    public async Task ShouldFilterEventInTenant(Option<TenantInfo> before, Option<TenantInfo> after)
    {
        await _receiver.Subscribe<Event>();

        var ev = new Event(11);
        using var scopeBefore = _tenantManager.MoveTo(before);
        await _sender.Publish(ev);

        using var scopeAfter = _tenantManager.MoveTo(after);
        if (after.IsAbsent || before.OrElse(TenantInfo.Public) == after.OrElse(TenantInfo.Public))
        {
            await _receiver.WaitForMessageOrFail(ev, _defaultTimeout);
        }
        else
        {
            await _receiver.FailIfMessageIsReceived(ev, _defaultTimeout);
        }
    }

    private async void Defer<T>(T command, Duration delay, Duration? tolerance = null) where T : ICommand
    {
        await Task.Run(async () =>
        {
            await Task.Delay((delay + (tolerance ?? _computationSlack)).ToTimeSpan());
            await _sender.Send(command);
        });
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await _sender.DisposeAsync();
        await _receiver.DisposeAsync();
    }
}
