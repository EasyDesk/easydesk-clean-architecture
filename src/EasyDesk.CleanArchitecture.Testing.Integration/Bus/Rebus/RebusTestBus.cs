using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.Tools.Collections;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Routing;
using System.Diagnostics;
using System.Threading.Channels;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;

public class RebusTestBus : ITestBus
{
    private static readonly Duration _defaultTimeout = Duration.FromSeconds(10);

    private readonly IBus _bus;
    private readonly ISet<Type> _subscriptions = new HashSet<Type>();
    private readonly Channel<IMessage> _messages = Channel.CreateUnbounded<IMessage>();
    private readonly IList<IMessage> _deadLetter = new List<IMessage>();
    private readonly Duration _timeout;

    public RebusTestBus(Action<RebusConfigurer> configureRebus, Duration? timeout = null)
    {
        _timeout = timeout ?? _defaultTimeout;

        var activator = new BuiltinHandlerActivator();
        activator.Handle<IMessage>(Handler);

        var configurer = Configure.With(activator);
        configureRebus(configurer);
        _bus = configurer.Start();
    }

    public async Task Subscribe<T>() where T : IEvent
    {
        await _bus.Subscribe<T>();
        _subscriptions.Add(typeof(T));
    }

    public async Task Unsubscribe<T>() where T : IEvent
    {
        await _bus.Unsubscribe<T>();
        _subscriptions.Remove(typeof(T));
    }

    public async Task Publish<T>(T message) where T : IEvent =>
        await _bus.Publish(message);

    public async Task Send<T>(T message) where T : ICommand =>
        await _bus.Send(message);

    public async Task Defer<T>(T message, Duration delay) where T : ICommand =>
        await _bus.Defer(delay.ToTimeSpan(), message);

    private async Task Handler(IMessage message) =>
        await _messages.Writer.WriteAsync(message);

    public async Task<T> WaitForMessageOrFail<T>(Func<T, bool> predicate, Duration? timeout = null) where T : IMessage
    {
        var (m, deadLetterIndex) = await WaitForMessage(predicate, timeout);
        deadLetterIndex.IfPresent(_deadLetter.RemoveAt);
        return m;
    }

    private async Task<(T, Option<int>)> WaitForMessage<T>(Func<T, bool> predicate, Duration? timeout = null)
    {
        var actualTimeout = timeout ?? _timeout;
        try
        {
            var messages = _deadLetter
                .Select((m, i) => (m, Some(i)))
                .ToAsyncEnumerable()
                .ThenConcat(() =>
                {
                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(actualTimeout.ToTimeSpan());
                    return _messages.Reader.ReadAllAsync(cts.Token).Select(m => (m, NoneT<int>()));
                });

            await foreach (var (m, deadLetterIndex) in messages)
            {
                if (m is T t && predicate(t))
                {
                    return (t, deadLetterIndex);
                }
                _deadLetter.Add(m);
            }

            throw new UnreachableException();
        }
        catch (OperationCanceledException)
        {
            throw new MessageNotReceivedWithinTimeoutException(actualTimeout, typeof(T));
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sub in _subscriptions)
        {
            await _bus.Unsubscribe(sub);
        }
        _bus.Dispose();
        GC.SuppressFinalize(this);
    }

    public static RebusTestBus CreateFromServices(IServiceProvider serviceProvider, string inputQueueAddress = null, Duration? defaultTimeout = null)
    {
        var options = serviceProvider.GetRequiredService<RebusMessagingOptions>();
        var serviceEndpoint = serviceProvider.GetRequiredService<RebusEndpoint>();
        var helperEndpoint = new RebusEndpoint(inputQueueAddress ?? GenerateNewRandomAddress());
        return new RebusTestBus(
            rebus =>
            {
                options.Apply(serviceProvider, helperEndpoint, rebus);
                rebus.Routing(r => r.Decorate(c => new TestRouterWrapper(c.Get<IRouter>(), serviceEndpoint)));
            },
            defaultTimeout);
    }

    private static string GenerateNewRandomAddress() => $"rebus-test-helper-{Guid.NewGuid()}";
}
