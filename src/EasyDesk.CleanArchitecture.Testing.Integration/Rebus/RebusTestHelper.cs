using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.Tools.Collections;
using NodaTime;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using System.Diagnostics;
using System.Threading.Channels;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Rebus;

public class RebusTestHelper : IAsyncDisposable
{
    private static readonly Duration _defaultTimeout = Duration.FromSeconds(10);

    private readonly IBus _bus;
    private readonly ISet<Type> _subscriptions = new HashSet<Type>();
    private readonly Channel<IMessage> _messages = Channel.CreateUnbounded<IMessage>();
    private readonly IList<IMessage> _deadLetter = new List<IMessage>();
    private readonly Duration _timeout;

    public RebusTestHelper(Action<RebusConfigurer> configureRebus, Duration? timeout = null)
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

    public async Task<T> WaitForMessageAfterDelayOrFail<T>(T message, Duration delay, Duration? timeout = null) =>
        await WaitForMessageAfterDelayOrFail<T>(m => m.Equals(message), delay, timeout);

    public async Task<T> WaitForMessageAfterDelayOrFail<T>(Duration delay, Duration? timeout = null) =>
        await WaitForMessageAfterDelayOrFail<T>(_ => true, delay, timeout);

    public async Task<T> WaitForMessageAfterDelayOrFail<T>(Func<T, bool> predicate, Duration delay, Duration? timeout = null)
    {
        await FailIfMessageIsReceivedWithin(predicate, delay);
        return await WaitForMessageOrFail(predicate, timeout);
    }

    public async Task FailIfMessageIsReceivedWithin<T>(T message, Duration? timeout = null) =>
        await FailIfMessageIsReceivedWithin<T>(m => m.Equals(message), timeout);

    public async Task FailIfMessageIsReceivedWithin<T>(Duration? timeout = null) =>
        await FailIfMessageIsReceivedWithin<T>(_ => true, timeout);

    public async Task FailIfMessageIsReceivedWithin<T>(Func<T, bool> predicate, Duration? timeout = null)
    {
        try
        {
            await WaitForMessageOrFail(predicate, timeout);
        }
        catch (MessageNotReceivedWithinTimeoutException)
        {
            return;
        }

        throw new UnexpectedMessageReceivedException(timeout ?? _defaultTimeout, typeof(T));
    }

    public async Task<T> WaitForMessageOrFail<T>(T message, Duration? timeout = null) =>
        await WaitForMessageOrFail<T>(x => x.Equals(message), timeout);

    public async Task<T> WaitForMessageOrFail<T>(Duration? timeout = null) =>
        await WaitForMessageOrFail<T>(_ => true, timeout);

    public async Task<T> WaitForMessageOrFail<T>(Func<T, bool> predicate, Duration? timeout = null)
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
}
