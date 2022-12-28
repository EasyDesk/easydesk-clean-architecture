using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using Newtonsoft.Json;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;

public class HttpSingleRequestExecutor<T> : HttpRequestExecutor<HttpSingleResponseWrapper<T>, HttpResponseMessage>
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public HttpSingleRequestExecutor(
        Func<HttpRequestMessage> requestFactory,
        ITestHttpAuthentication testHttpAuthentication,
        HttpClient httpClient,
        JsonSerializerSettings jsonSerializerSettings)
        : base(requestFactory, testHttpAuthentication)
    {
        _httpClient = httpClient;
        _jsonSerializerSettings = jsonSerializerSettings;
    }

    public HttpSingleResponseWrapper<T> PollUntil(Func<T, bool> predicate, Duration? interval, Duration? timeout = null) =>
        PollUntil(async httpRM => predicate(await httpRM.AsData()), interval, timeout);

    public HttpSingleResponseWrapper<T> PollWhile(Func<T, bool> predicate, Duration? interval, Duration? timeout = null) =>
        PollWhile(async httpRM => predicate(await httpRM.AsData()), interval, timeout);

    protected override Task<HttpResponseMessage> MakeRequest() =>
        _httpClient.SendAsync(CreateRequest());

    protected override HttpSingleResponseWrapper<T> Wrap(AsyncFunc<HttpResponseMessage> request) =>
        new(request, _jsonSerializerSettings);
}
