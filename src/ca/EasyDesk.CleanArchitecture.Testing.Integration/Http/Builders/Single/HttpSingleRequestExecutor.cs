using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using Newtonsoft.Json;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;

public class HttpSingleRequestExecutor<T>
    : HttpRequestExecutor<HttpSingleResponseWrapper<T>, ImmutableHttpResponseMessage, HttpSingleRequestExecutor<T>>
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public HttpSingleRequestExecutor(
        string endpoint,
        HttpMethod method,
        ITestHttpAuthentication testHttpAuthentication,
        HttpClient httpClient,
        JsonSerializerSettings jsonSerializerSettings)
        : base(endpoint, method, testHttpAuthentication)
    {
        _httpClient = httpClient;
        _jsonSerializerSettings = jsonSerializerSettings;
    }

    public HttpSingleResponseWrapper<T> PollUntil(Func<T, bool> predicate, Duration? interval, Duration? timeout = null) =>
        PollUntil(async wrapped => predicate(await wrapped.AsData()), interval, timeout);

    public HttpSingleResponseWrapper<T> PollWhile(Func<T, bool> predicate, Duration? interval, Duration? timeout = null) =>
        PollWhile(async wrapped => predicate(await wrapped.AsData()), interval, timeout);

    protected override async Task<ImmutableHttpResponseMessage> MakeRequest(CancellationToken timeoutToken)
    {
        using var req = CreateRequest().ToHttpRequestMessage();
        using var res = await _httpClient.SendAsync(req, timeoutToken);
        return await ImmutableHttpResponseMessage.From(res);
    }

    protected override HttpSingleResponseWrapper<T> Wrap(AsyncFunc<ImmutableHttpResponseMessage> request) =>
        new(request, _jsonSerializerSettings);
}
