using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Commons.Polling;

public class Polling<T>
{
    public static readonly Duration DefaultTimeout = Duration.FromSeconds(10);
    public static readonly Duration DefaultInterval = Duration.FromMilliseconds(200);
    private readonly AsyncFunc<CancellationToken, T> _poller;
    private readonly Duration _timeout;
    private readonly Duration _interval;

    public Polling(
        AsyncFunc<CancellationToken, T> poller,
        Duration? timeout = null,
        Duration? interval = null)
    {
        _poller = poller;
        _timeout = timeout ?? DefaultTimeout;
        _interval = interval ?? DefaultInterval;
    }

    public async Task<T> PollWhile(AsyncFunc<T, bool> predicate)
    {
        using var cts = new CancellationTokenSource(_timeout.ToTimeSpan());
        var attempts = 1;
        var actualInterval = _interval.ToTimeSpan();
        var pollResult = await _poller(cts.Token);
        while (await predicate(pollResult))
        {
            if (cts.IsCancellationRequested)
            {
                throw new PollingFailedException(attempts, _timeout);
            }

            try
            {
                await Task.Delay(actualInterval, cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new PollingFailedException(attempts, _timeout);
            }

            attempts++;
            pollResult = await _poller(cts.Token);
        }
        return pollResult;
    }

    public Task<T> PollWhile(Func<T, bool> predicate) =>
        PollWhile(t => Task.FromResult(predicate(t)));

    public Task<T> PollUntil(AsyncFunc<T, bool> cond) =>
        PollWhile(async r => !await cond(r));

    public Task<T> PollUntil(Func<T, bool> cond) =>
        PollUntil(r => Task.FromResult(cond(r)));

    public Polling<R> Map<R>(Func<T, R> mapper) =>
        new(async token => mapper(await _poller(token)), _timeout, _interval);
}
