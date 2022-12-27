using Newtonsoft.Json;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public class HttpSingleRequestExecutor<T>
{
    private readonly HttpClient _httpClient;
    private readonly HttpRequestMessage _request;
    private readonly JsonSerializerSettings _serializerSettings;
    private readonly IClock _clock = SystemClock.Instance;
    private static readonly Duration _defaultPollTimeout = Duration.FromSeconds(5);
    private static readonly Duration _defaultRequestInterval = Duration.FromMilliseconds(200);

    public HttpSingleRequestExecutor(HttpClient httpClient, HttpRequestMessage httpRequestMessage, JsonSerializerSettings serializerSettings)
    {
        _httpClient = httpClient;
        _request = httpRequestMessage;
        _serializerSettings = serializerSettings;
    }

    public HttpResponseWrapper<T> Send() =>
        Wrap(() => _httpClient.SendAsync(_request));

    public HttpResponseWrapper<T> PollUntil(AsyncFunc<HttpResponseWrapper<T>, bool> condition, Option<Duration> timeout = default) =>
        PollWhile(async httpRM => !await condition(httpRM), timeout);

    public HttpResponseWrapper<T> PollWhile(AsyncFunc<HttpResponseWrapper<T>, bool> condition, Option<Duration> timeout = default) =>
        Wrap(async () =>
        {
            var startPollTime = _clock.GetCurrentInstant();
            var actualTimeout = timeout.OrElse(_defaultPollTimeout);
            var polls = 1;
            async Task<HttpResponseMessage> Poll(bool clone = true)
            {
                var request = clone ? await _request.Clone() : _request;
                var cts = new CancellationTokenSource();
                cts.CancelAfter(actualTimeout.ToTimeSpan());
                return await _httpClient.SendAsync(request, cts.Token);
            }
            var lastPollTime = startPollTime;
            var message = await Poll(clone: false);
            while (await condition(Wrap(message)))
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
                message = await Poll();
            }
            return message;
        });

    private HttpResponseWrapper<T> Wrap(AsyncFunc<HttpResponseMessage> message) =>
        new(message, _serializerSettings);

    private HttpResponseWrapper<T> Wrap(HttpResponseMessage message) =>
        new(message, _serializerSettings);
}

public static class HttpRequestExecutorExtensions
{
    public static Task<T> PollUntil<T>(this HttpSingleRequestExecutor<T> builder, Func<T, bool> condition) =>
        builder.PollUntil(r => r.Check(condition)).AsData();
}
