using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.Commons.Options;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Bus;

public interface ITestBusEndpoint : IAsyncDisposable
{
    Task Subscribe<T>() where T : IEvent;

    Task Unsubscribe<T>() where T : IEvent;

    Task Defer<T>(T message, Duration delay) where T : ICommand;

    Task Publish<T>(T message) where T : IEvent;

    Task Send<T>(T message) where T : ICommand;

    Task<T> WaitForMessageOrFail<T>(Func<T, bool> predicate, Duration? timeout = null) where T : IMessage;

    Task FailIfMessageIsReceived<T>(Func<T, bool> predicate, Duration? timeout = null) where T : IMessage;

    public async Task<T> WaitForMessageOrFail<T>(T message, Duration? timeout = null) where T : IMessage =>
        await WaitForMessageOrFail<T>(x => x.Equals(message), timeout);

    public async Task<T> WaitForMessageOrFail<T>(Duration? timeout = null) where T : IMessage =>
        await WaitForMessageOrFail<T>(_ => true, timeout);

    public async Task FailIfMessageIsReceived<T>(T message, Duration? timeout = null) where T : IMessage =>
        await FailIfMessageIsReceived<T>(m => m.Equals(message), timeout);

    public async Task FailIfMessageIsReceived<T>(Duration? timeout = null) where T : IMessage =>
        await FailIfMessageIsReceived<T>(_ => true, timeout);

    public async Task<T> WaitForMessageAfterDelayOrFail<T>(T message, Duration delay, Duration? timeout = null) where T : IMessage =>
        await WaitForMessageAfterDelayOrFail<T>(m => m.Equals(message), delay, timeout);

    public async Task<T> WaitForMessageAfterDelayOrFail<T>(Duration delay, Duration? timeout = null) where T : IMessage =>
        await WaitForMessageAfterDelayOrFail<T>(_ => true, delay, timeout);

    public async Task<T> WaitForMessageAfterDelayOrFail<T>(Func<T, bool> predicate, Duration delay, Duration? timeout = null) where T : IMessage
    {
        await FailIfMessageIsReceived(predicate, delay);
        return await WaitForMessageOrFail(predicate, timeout);
    }

    public IAsyncEnumerable<T> WaitForMessagesUntilQuiet<T>(Duration? timeout = null) where T : IMessage =>
        WaitForMessagesUntilQuiet<T>(_ => true, timeout);

    public async IAsyncEnumerable<T> WaitForMessagesUntilQuiet<T>(Func<T, bool> predicate, Duration? timeout = null) where T : IMessage
    {
        while (true)
        {
            Option<T> message;

            try
            {
                message = Some(await WaitForMessageOrFail(predicate, timeout));
            }
            catch (MessageNotReceivedWithinTimeoutException)
            {
                message = None;
            }

            if (message.IsPresent)
            {
                yield return message.Value;
            }
            else
            {
                yield break;
            }
        }
    }
}
