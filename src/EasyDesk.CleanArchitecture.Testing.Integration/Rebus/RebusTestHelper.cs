using EasyDesk.CleanArchitecture.Application.Cqrs.Commands;
using EasyDesk.CleanArchitecture.Application.Cqrs.Events;
using EasyDesk.CleanArchitecture.Application.Messaging;
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

    public async Task Subscribe<T>() where T : IMessage, IEvent
    {
        await _bus.Subscribe<T>();
        _subscriptions.Add(typeof(T));
    }

    public async Task Unsubscribe<T>() where T : IMessage, IEvent
    {
        await _bus.Unsubscribe<T>();
        _subscriptions.Remove(typeof(T));
    }

    public async Task Publish<T>(T message) where T : IMessage, IEvent
    {
        await _bus.Publish(message);
    }

    public async Task Send<T>(T message) where T : IMessage, ICommand
    {
        await _bus.Send(message);
    }

    public async Task Defer<T>(T message, Duration delay) where T : IMessage, ICommand
    {
        await _bus.Defer(delay.ToTimeSpan(), message);
    }

    private async Task Handler(IMessage message)
    {
        await _messages.Writer.WriteAsync(message);
    }

    public async Task<T> WaitForMessageOrFail<T>(T message, Duration? timeout = null)
    {
        return await WaitForMessageOrFail<T>(x => x.Equals(message), timeout);
    }

    public async Task<T> WaitForMessageOrFail<T>(Duration? timeout = null)
    {
        return await WaitForMessageOrFail<T>(_ => true, timeout);
    }

    public async Task<T> WaitForMessageOrFail<T>(Func<T, bool> predicate, Duration? timeout = null)
    {
        var actualTimeout = timeout ?? _timeout;
        try
        {
            var messages = _deadLetter
                .ToAsyncEnumerable()
                .ThenConcat(() =>
                {
                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(actualTimeout.ToTimeSpan());
                    return _messages.Reader.ReadAllAsync(cts.Token);
                });

            await foreach (var m in messages)
            {
                if (m is T t && predicate(t))
                {
                    return t;
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
