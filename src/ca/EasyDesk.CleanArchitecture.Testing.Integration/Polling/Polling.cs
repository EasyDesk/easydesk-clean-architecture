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
    private AsyncFunc<int, T> _fallback;
    private Duration _timeout;
    private Duration _interval;

    internal Polling(
        AsyncFunc<CancellationToken, T> poller,
        AsyncFunc<int, T>? fallback = null,
        Duration? timeout = null,
        Duration? interval = null)
    {
        _poller = poller;
        _fallback = fallback ?? DefaultFallback;
        _timeout = timeout ?? DefaultTimeout;
        _interval = interval ?? DefaultInterval;
    }

    private Task<T> DefaultFallback(int attempts) => throw new PollingFailedException(attempts, _timeout);

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

    public Polling<T> WithFallback(AsyncFunc<int, T> fallback)
    {
        _fallback = fallback;
        return this;
    }

    public Polling<T> WithFallback(Func<int, T> fallback) =>
        WithFallback(a => Task.FromResult(fallback(a)));

    public async Task<T> While(AsyncFunc<T, bool> predicate)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.CancelAfter(_timeout.ToTimeSpan());
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
        catch (OperationCanceledException) when (!TestContext.Current.CancellationToken.IsCancellationRequested)
        {
            return await _fallback(attempts);
        }
    }

    public Task<T> While(Func<T, bool> predicate) =>
        While(t => Task.FromResult(predicate(t)));

    public Task<T> Until(AsyncFunc<T, bool> cond) =>
        While(async r => !await cond(r));

    public Task<T> Until(Func<T, bool> cond) =>
        Until(r => Task.FromResult(cond(r)));

    public Polling<R> Map<R>(AsyncFunc<T, R> mapper) =>
        new(t => _poller(t).FlatMap(mapper), a => _fallback(a).FlatMap(mapper), _timeout, _interval);

    public Polling<R> Map<R>(Func<T, R> mapper) =>
        new(t => _poller(t).Map(mapper), a => _fallback(a).Map(mapper), _timeout, _interval);

    public Task<bool> PropertyHolds(AsyncFunc<T, bool> property) =>
        Map(property).WithFallback(_ => true).While(It);

    public Task<bool> EventuallyPropertyHolds(AsyncFunc<T, bool> property) =>
        Map(property).WithFallback(_ => PropertyHolds(property)).Until(It);

    public Task<bool> PropertyHolds(Func<T, bool> property) =>
        PropertyHolds(t => Task.FromResult(property(t)));

    public Task<bool> EventuallyPropertyHolds(Func<T, bool> property) =>
        EventuallyPropertyHolds(t => Task.FromResult(property(t)));

    public async Task EnsureInvariant(AsyncFunc<T, bool> invariant)
    {
        var ok = await PropertyHolds(invariant);
        if (ok)
        {
            return;
        }

        throw new InvariantDidNotHoldException();
    }

    public Task EnsureInvariant(Func<T, bool> invariant) =>
        EnsureInvariant(t => Task.FromResult(invariant(t)));

    public Task EnsureDoesNotThrow<E>(AsyncAction<T> action) where E : Exception =>
        EnsureInvariant(DoesNotThrow<E>(action));

    public Task EnsureDoesNotThrow<E>(Action<T> action) where E : Exception =>
        EnsureInvariant(DoesNotThrow<E>(action));

    public async Task EventuallyEnsureInvariant(AsyncFunc<T, bool> invariant)
    {
        var ok = await EventuallyPropertyHolds(invariant);
        if (ok)
        {
            return;
        }

        throw new InvariantDidNotHoldException();
    }

    public Task EventuallyEnsureInvariant(Func<T, bool> invariant) =>
        EventuallyEnsureInvariant(t => Task.FromResult(invariant(t)));

    public Task EventuallyEnsureDoesNotThrow<E>(AsyncAction<T> action) where E : Exception =>
        EventuallyEnsureInvariant(DoesNotThrow<E>(action));

    public Task EventuallyEnsureDoesNotThrow<E>(Action<T> action) where E : Exception =>
        EventuallyEnsureInvariant(DoesNotThrow<E>(action));

    private Func<T, bool> DoesNotThrow<E>(Action<T> action) where E : Exception => t =>
    {
        try
        {
            action(t);
            return true;
        }
        catch (E)
        {
            return false;
        }
    };

    private AsyncFunc<T, bool> DoesNotThrow<E>(AsyncAction<T> action) where E : Exception => async t =>
    {
        try
        {
            await action(t);
            return true;
        }
        catch (E)
        {
            return false;
        }
    };

    public Task<T> UntilDoesNotThrow<E>(Action<T> action) where E : Exception => Until(DoesNotThrow<E>(action));

    public Task<T> UntilDoesNotThrow(Action<T> action) => UntilDoesNotThrow<Exception>(action);

    public Task<T> UntilDoesNotThrow<E>(AsyncAction<T> action) where E : Exception => Until(DoesNotThrow<E>(action));

    public Task<T> UntilDoesNotThrow(AsyncAction<T> action) => UntilDoesNotThrow<Exception>(action);

    public Task<T> UntilThrows<E>(Action<T> action) where E : Exception => While(DoesNotThrow<E>(action));

    public Task<T> UntilThrows<E>(AsyncAction<T> action) where E : Exception => While(DoesNotThrow<E>(action));
}
