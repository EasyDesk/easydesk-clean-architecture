using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using Newtonsoft.Json;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;

public class HttpSingleRequestExecutor<T>
    : HttpRequestExecutor<T, HttpSingleResponseWrapper<T>, HttpResponseMessage>
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public HttpSingleRequestExecutor(
        HttpRequestMessage request,
        ITestHttpAuthentication testHttpAuthentication,
        HttpClient httpClient,
        JsonSerializerSettings jsonSerializerSettings)
        : base(request, testHttpAuthentication)
    {
        _httpClient = httpClient;
        _jsonSerializerSettings = jsonSerializerSettings;
    }

    public HttpSingleResponseWrapper<T> PollUntil(Func<T, bool> predicate, Option<Duration> timeout = default) =>
        PollUntil(async httpRM => predicate(await httpRM.AsData()), timeout);

    public HttpSingleResponseWrapper<T> PollWhile(Func<T, bool> predicate, Option<Duration> timeout = default) =>
        PollWhile(async httpRM => predicate(await httpRM.AsData()), timeout);

    protected override Task<HttpResponseMessage> Send(HttpRequestMessage request, CancellationToken cancellationToken) =>
        _httpClient.SendAsync(request, cancellationToken);

    protected override HttpSingleResponseWrapper<T> Wrap(AsyncFunc<HttpResponseMessage> request) =>
        new(request, _jsonSerializerSettings);
}
