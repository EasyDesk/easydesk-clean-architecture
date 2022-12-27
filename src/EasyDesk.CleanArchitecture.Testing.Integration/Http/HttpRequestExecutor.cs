using Newtonsoft.Json;
using NodaTime;
using System.Web;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public class HttpRequestExecutor
{
    private readonly HttpClient _httpClient;
    private readonly HttpRequestMessage _request;
    private readonly JsonSerializerSettings _serializerSettings;
    private readonly IClock _clock = SystemClock.Instance;
    private static readonly Duration _defaultPollTimeout = Duration.FromSeconds(5);
    private static readonly Duration _defaultRequestInterval = Duration.FromMilliseconds(200);

    public HttpRequestExecutor(HttpClient httpClient, HttpRequestMessage httpRequestMessage, JsonSerializerSettings serializerSettings)
    {
        _httpClient = httpClient;
        _request = httpRequestMessage;
        _serializerSettings = serializerSettings;
    }

    public async Task<HttpResponseWrapper> Send() =>
        Wrap(await _httpClient.SendAsync(_request));

    public Task<HttpResponseWrapper> PollUntil(AsyncFunc<HttpResponseWrapper, bool> condition, Option<Duration> timeout = default) =>
        PollWhile(async httpRM => !await condition(httpRM), timeout);

    public async Task<HttpResponseWrapper> PollWhile(AsyncFunc<HttpResponseWrapper, bool> condition, Option<Duration> timeout = default)
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
        return Wrap(message);
    }

    public async Task<HttpPaginatedResponsesCollection> SendForEveryPage()
    {
        var page = WrapPaginated(await _httpClient.SendAsync(_request));
        await page.EnsureSuccess();
        var (pageIndex, pageCount) = await page.GetIndex();
        var pages = new List<HttpPaginatedResponseWrapper>()
        {
            page
        };
        for (var i = pageIndex + 1; i < pageCount; i++)
        {
            var req = await _request.Clone();
            var query = HttpUtility.ParseQueryString(req.RequestUri.Query);
            query[nameof(pageIndex)] = i.ToString();
            var uriBuilder = new UriBuilder(req.RequestUri)
            {
                Query = query.ToString()
            };
            req.RequestUri = uriBuilder.Uri;
            var pageResponse = WrapPaginated(await _httpClient.SendAsync(req));
            await pageResponse.EnsureSuccess();
            pages.Add(pageResponse);
        }
        return new HttpPaginatedResponsesCollection(pages);
    }

    private HttpResponseWrapper Wrap(HttpResponseMessage message) => new(message, _serializerSettings);

    private HttpPaginatedResponseWrapper WrapPaginated(HttpResponseMessage message) => new(message, _serializerSettings);
}

public static class HttpRequestExecutorExtensions
{
    public static Task<T> PollUntil<T>(this HttpRequestExecutor builder, Func<T, bool> condition) =>
        builder.PollUntil(r => r.Check(condition)).AsData<T>();
}
