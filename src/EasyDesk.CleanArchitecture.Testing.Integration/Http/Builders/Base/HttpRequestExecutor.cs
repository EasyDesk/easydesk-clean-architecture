using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public abstract class HttpRequestExecutor<W, I> : HttpRequestBuilder<HttpRequestExecutor<W, I>>
{
    private static readonly Duration _defaultPollTimeout = Duration.FromSeconds(5);
    private static readonly Duration _defaultRequestInterval = Duration.FromMilliseconds(200);

    public HttpRequestExecutor(
        Func<HttpRequestMessage> request,
        ITestHttpAuthentication testHttpAuthentication)
        : base(request, testHttpAuthentication)
    {
    }

    public W Send() => Wrap(MakeRequest);

    public W PollWhile(AsyncFunc<W, bool> predicate, Duration? interval = null, Duration? timeout = null) =>
        Wrap(async () =>
        {
            var actualTimeout = timeout ?? _defaultPollTimeout;
            var cts = new CancellationTokenSource(actualTimeout.ToTimeSpan());
            var attempts = 1;
            var actualInterval = (interval ?? _defaultRequestInterval).ToTimeSpan();
            var message = await MakeRequest();
            while (await predicate(Wrap(() => Task.FromResult(message))))
            {
                if (cts.IsCancellationRequested)
                {
                    throw new PollingFailedException(attempts, actualTimeout);
                }

                try
                {
                    await Task.Delay(actualInterval, cts.Token);
                }
                catch (TaskCanceledException)
                {
                    throw new PollingFailedException(attempts, actualTimeout);
                }

                attempts++;
                message = await MakeRequest();
            }
            return message;
        });

    public W PollUntil(AsyncFunc<W, bool> predicate, Duration? interval = null, Duration? timeout = null) =>
        PollWhile(async httpRM => !await predicate(httpRM), interval, timeout);

    protected abstract Task<I> MakeRequest();

    protected abstract W Wrap(AsyncFunc<I> request);
}
