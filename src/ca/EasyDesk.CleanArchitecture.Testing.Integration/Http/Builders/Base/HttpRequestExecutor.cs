using EasyDesk.CleanArchitecture.Testing.Integration.Polling;
using EasyDesk.Commons.Tasks;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public abstract class HttpRequestExecutor<W, I, E> : HttpRequestBuilder<E>
    where E : HttpRequestExecutor<W, I, E>
{
    private static readonly Duration _defaultPollingInterval = Duration.FromMilliseconds(200);

    protected HttpRequestExecutor(
        string endpoint,
        HttpMethod method,
        ITestHttpAuthentication testHttpAuthentication)
        : base(endpoint, method, testHttpAuthentication)
    {
    }

    protected abstract Task<I> MakeRequest(CancellationToken timeoutToken);

    protected abstract W Wrap(AsyncFunc<I> request);

    public W Send(Duration? timeout = null) => Wrap(async () =>
    {
        var actualTimeout = timeout ?? Timeout;
        using var cts = new CancellationTokenSource(actualTimeout.ToTimeSpan());
        return await MakeRequest(cts.Token);
    });

    public W PollWhile(AsyncFunc<W, bool> predicate, Duration? interval = null, Duration? timeout = null) =>
        PollWrapper((p, cond) => p.While(cond), predicate, interval, timeout);

    public W PollUntil(AsyncFunc<W, bool> predicate, Duration? interval = null, Duration? timeout = null) =>
        PollWrapper((p, cond) => p.Until(cond), predicate, interval, timeout);

    private W PollWrapper(
        AsyncFunc<Polling<I>, AsyncFunc<I, bool>, I> pollingType,
        AsyncFunc<W, bool> predicate,
        Duration? interval = null,
        Duration? timeout = null) =>
        Wrap(async () =>
        {
            var actualTimeout = timeout ?? Timeout;
            var actualInterval = interval ?? _defaultPollingInterval;
            var polling = Poll
                .Async(token => MakeRequest(token))
                .WithTimeout(actualTimeout)
                .WithInterval(actualInterval);
            return await pollingType(polling, i => predicate(Wrap(() => Task.FromResult(i))));
        });
}
