using EasyDesk.CleanArchitecture.Testing.Integration.Polling;
using EasyDesk.Commons.Tasks;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public abstract class HttpRequestExecutor<W, I, E>
    where E : HttpRequestExecutor<W, I, E>
{
    private static readonly Duration _defaultPollingInterval = Duration.FromMilliseconds(200);

    protected HttpRequestExecutor(HttpRequestBuilder httpRequestBuilder)
    {
        HttpRequestBuilder = httpRequestBuilder;
    }

    protected HttpRequestBuilder HttpRequestBuilder { get; }

    protected abstract Task<I> MakeRequest(CancellationToken timeoutToken);

    protected abstract W Wrap(AsyncFunc<I> request);

    public E With(Action<HttpRequestBuilder> configure)
    {
        configure(HttpRequestBuilder);
        return (E)this;
    }

    public W Send(Duration? timeout = null) => Wrap(async () =>
    {
        var actualTimeout = timeout ?? HttpRequestBuilder.RequestTimeout;
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.CancelAfter(actualTimeout.ToTimeSpan());
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
            var actualTimeout = timeout ?? HttpRequestBuilder.RequestTimeout;
            var actualInterval = interval ?? _defaultPollingInterval;
            var polling = Poll
                .Async(token => MakeRequest(token))
                .WithTimeout(actualTimeout)
                .WithInterval(actualInterval);
            return await pollingType(polling, i => predicate(Wrap(() => Task.FromResult(i))));
        });
}
