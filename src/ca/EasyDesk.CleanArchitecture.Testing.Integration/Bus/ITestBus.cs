using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Bus;

public interface ITestBus : IAsyncDisposable
{
    Task Subscribe<T>() where T : IEvent;

    Task Unsubscribe<T>() where T : IEvent;

    Task Defer<T>(T message, Duration delay, TenantId tenant) where T : ICommand;

    Task Publish<T>(T message, TenantId tenant) where T : IEvent;

    Task Send<T>(T message, TenantId tenant) where T : ICommand;

    Task Defer<T>(T message, Duration delay) where T : ICommand;

    Task Publish<T>(T message) where T : IEvent;

    Task Send<T>(T message) where T : ICommand;

    Task<T> WaitForMessageOrFail<T>(Func<T, bool> predicate, Duration? timeout = null) where T : IMessage;

    public async Task<T> WaitForMessageOrFail<T>(T message, Duration? timeout = null) where T : IMessage =>
        await WaitForMessageOrFail<T>(x => x.Equals(message), timeout);

    public async Task<T> WaitForMessageOrFail<T>(Duration? timeout = null) where T : IMessage =>
        await WaitForMessageOrFail<T>(_ => true, timeout);

    public async Task FailIfMessageIsReceivedWithin<T>(Func<T, bool> predicate, Duration? timeout = null) where T : IMessage
    {
        try
        {
            await WaitForMessageOrFail(predicate, timeout);
        }
        catch (MessageNotReceivedWithinTimeoutException)
        {
            return;
        }

        throw new UnexpectedMessageReceivedException(typeof(T));
    }

    public async Task FailIfMessageIsReceivedWithin<T>(T message, Duration? timeout = null) where T : IMessage =>
        await FailIfMessageIsReceivedWithin<T>(m => m.Equals(message), timeout);

    public async Task FailIfMessageIsReceivedWithin<T>(Duration? timeout = null) where T : IMessage =>
        await FailIfMessageIsReceivedWithin<T>(_ => true, timeout);

    public async Task<T> WaitForMessageAfterDelayOrFail<T>(T message, Duration delay, Duration? timeout = null) where T : IMessage =>
        await WaitForMessageAfterDelayOrFail<T>(m => m.Equals(message), delay, timeout);

    public async Task<T> WaitForMessageAfterDelayOrFail<T>(Duration delay, Duration? timeout = null) where T : IMessage =>
        await WaitForMessageAfterDelayOrFail<T>(_ => true, delay, timeout);

    public async Task<T> WaitForMessageAfterDelayOrFail<T>(Func<T, bool> predicate, Duration delay, Duration? timeout = null) where T : IMessage
    {
        await FailIfMessageIsReceivedWithin(predicate, delay);
        return await WaitForMessageOrFail(predicate, timeout);
    }
}
