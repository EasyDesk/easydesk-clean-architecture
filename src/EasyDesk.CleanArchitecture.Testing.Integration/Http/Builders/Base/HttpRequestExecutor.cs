using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public abstract class HttpRequestExecutor<W, I>
    : HttpRequestBuilder<HttpRequestExecutor<W, I>>
{
    private readonly IClock _clock = SystemClock.Instance;
    private static readonly Duration _defaultPollTimeout = Duration.FromSeconds(5);
    private static readonly Duration _defaultRequestInterval = Duration.FromMilliseconds(200);

    public HttpRequestExecutor(
        HttpRequestMessage request,
        ITestHttpAuthentication testHttpAuthentication)
        : base(request, testHttpAuthentication)
    {
    }

    protected abstract Task<I> Send(HttpRequestMessage request, CancellationToken cancellationToken);

    protected abstract W Wrap(AsyncFunc<I> request);

    public W PollWhile(AsyncFunc<W, bool> predicate, Option<Duration> timeout = default) =>
        Wrap(async () =>
        {
            var startPollTime = _clock.GetCurrentInstant();
            var actualTimeout = timeout.OrElse(_defaultPollTimeout);
            var polls = 1;
            async Task<I> Poll(bool clone)
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(actualTimeout.ToTimeSpan());
                return await Send(await Request, cts.Token);
            }
            var lastPollTime = startPollTime;
            var message = await Poll(clone: false);
            while (await predicate(Wrap(() => Task.FromResult(message))))
            {
                var elapsed = _clock.GetCurrentInstant() - lastPollTime;
                if (elapsed >= actualTimeout)
                {
                    var elapsedTotal = _clock.GetCurrentInstant() - startPollTime;
                    throw new TaskCanceledException($"Polling timed out. Polls attempted: {polls} polls in {elapsedTotal.TotalSeconds:0.000} seconds");
                }
                actualTimeout -= elapsed;
                polls++;
                await Task.Delay(_defaultRequestInterval.ToTimeSpan());
                lastPollTime = _clock.GetCurrentInstant();
                message = await Poll(clone: true);
            }
            return message;
        });

    public W Send() => Wrap(async () => await Send(await Request, CancellationToken.None));

    public W PollUntil(AsyncFunc<W, bool> predicate, Option<Duration> timeout = default) =>
        PollWhile(async w => !await predicate(w), timeout);
}
