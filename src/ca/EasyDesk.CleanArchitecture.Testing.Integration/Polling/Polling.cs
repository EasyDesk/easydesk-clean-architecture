using EasyDesk.Commons.Tasks;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Polling;

public static class Poll
{
    public static Polling<T> Async<T>(AsyncFunc<CancellationToken, T> poller) => new(poller);

    public static Polling<T> Sync<T>(Func<T> poller) => Async(_ => Task.FromResult(poller()));
}

public sealed class Polling<T>
{
    public static readonly Duration DefaultTimeout = Duration.FromSeconds(10);
    public static readonly Duration DefaultInterval = Duration.FromMilliseconds(200);
    private readonly AsyncFunc<CancellationToken, T> _poller;
    private Duration _timeout;
    private Duration _interval;

    internal Polling(
        AsyncFunc<CancellationToken, T> poller,
        Duration? timeout = null,
        Duration? interval = null)
    {
        _poller = poller;
        _timeout = timeout ?? DefaultTimeout;
        _interval = interval ?? DefaultInterval;
    }

    public Polling<T> WithTimeout(Duration timeout)
    {
        _timeout = timeout;
        return this;
    }

    public Polling<T> WithInterval(Duration interval)
    {
        _interval = interval;
        return this;
    }

    public async Task<T> While(AsyncFunc<T, bool> predicate)
    {
        using var cts = new CancellationTokenSource(_timeout.ToTimeSpan());
        var attempts = 1;
        var actualInterval = _interval.ToTimeSpan();
        try
        {
            var pollResult = await _poller(cts.Token);
            while (await predicate(pollResult))
            {
                await Task.Delay(actualInterval, cts.Token);

                attempts++;
                pollResult = await _poller(cts.Token);
            }
            return pollResult;
        }
        catch (OperationCanceledException)
        {
            throw new PollingFailedException(attempts, _timeout);
        }
    }

    public Task<T> While(Func<T, bool> predicate) =>
        While(t => Task.FromResult(predicate(t)));

    public Task<T> Until(AsyncFunc<T, bool> cond) =>
        While(async r => !await cond(r));

    public Task<T> Until(Func<T, bool> cond) =>
        Until(r => Task.FromResult(cond(r)));

    public Polling<R> Map<R>(Func<T, R> mapper) =>
        new(async token => mapper(await _poller(token)), _timeout, _interval);
}
