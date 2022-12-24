using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.CleanArchitecture.Web.Versioning;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NodaTime;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public class HttpRequestBuilder
{
    private static readonly Duration _defaultPollTimeout = Duration.FromSeconds(5);
    private static readonly Duration _defaultRequestInterval = Duration.FromMilliseconds(200);
    private readonly HttpRequestMessage _request;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _settings;
    private readonly ITestHttpAuthentication _testHttpAuthentication;
    private readonly IClock _clock = SystemClock.Instance;

    public HttpRequestBuilder(
        HttpRequestMessage request,
        HttpClient httpClient,
        JsonSerializerSettings settings,
        ITestHttpAuthentication testHttpAuthentication)
    {
        _request = request;
        _httpClient = httpClient;
        _settings = settings;
        _testHttpAuthentication = testHttpAuthentication;
    }

    public HttpRequestBuilder Headers(Action<HttpRequestHeaders> configureHeaders)
    {
        configureHeaders(_request.Headers);
        return this;
    }

    public HttpRequestBuilder WithApiVersion(ApiVersion version) =>
        Headers(h => h.Replace(ApiVersioningUtils.VersionHeader, version.ToString()));

    public HttpRequestBuilder Tenant(string tenantId) =>
        Headers(h => h.Replace(MultitenancyDefaults.TenantIdHttpHeader, tenantId));

    public HttpRequestBuilder NoTenant() =>
        Headers(h => h.Remove(MultitenancyDefaults.TenantIdHttpHeader));

    public HttpRequestBuilder NoAuthentication()
    {
        _testHttpAuthentication.RemoveAuthentication(this);
        return this;
    }

    public HttpRequestBuilder Authenticate(IEnumerable<Claim> identity)
    {
        _testHttpAuthentication.ConfigureAuthentication(this, identity);
        return this;
    }

    public HttpRequestBuilder AuthenticateAs(string userId) =>
        Authenticate(new Claim[] { new Claim(ClaimTypes.NameIdentifier, userId) });

    public async Task<HttpResponseMessage> AsHttpResponseMessage() =>
        await _httpClient.SendAsync(_request);

    public Task<ResponseDto<T>> PollWhile<T>(Func<ResponseDto<T>, bool> condition, Option<Duration> timeout = default) =>
        PollWhile<T>(r => Task.FromResult(condition(r)), timeout);

    public async Task<ResponseDto<T>> PollWhile<T>(AsyncFunc<ResponseDto<T>, bool> condition, Option<Duration> timeout = default) =>
        await ParseContent<T>(await PollWhile(async hrm => await condition(await ParseContent<T>(hrm)), timeout));

    public Task<HttpResponseMessage> PollWhile(Func<HttpResponseMessage, bool> condition, Option<Duration> timeout = default) =>
        PollWhile(hrm => Task.FromResult(condition(hrm)), timeout);

    public async Task<HttpResponseMessage> PollWhile(AsyncFunc<HttpResponseMessage, bool> condition, Option<Duration> timeout = default)
    {
        var startPollTime = _clock.GetCurrentInstant();
        var actualTimeout = timeout.OrElse(_defaultPollTimeout);
        var polls = 1;
        async Task<HttpResponseMessage> Poll(bool clone = true)
        {
            var request = clone ? await Clone(_request) : _request;
            var cts = new CancellationTokenSource();
            cts.CancelAfter(actualTimeout.ToTimeSpan());
            return await _httpClient.SendAsync(request, cts.Token);
        }
        var lastPollTime = startPollTime;
        var message = await Poll(clone: false);
        while (await condition(message))
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
    }

    public Task<ResponseDto<T>> PollUntil<T>(Func<ResponseDto<T>, bool> condition, Option<Duration> timeout = default) =>
        PollUntil<T>(r => Task.FromResult(condition(r)), timeout);

    public async Task<ResponseDto<T>> PollUntil<T>(AsyncFunc<ResponseDto<T>, bool> condition, Option<Duration> timeout = default) =>
        await ParseContent<T>(await PollUntil(async hrm => await condition(await ParseContent<T>(hrm)), timeout));

    public Task<HttpResponseMessage> PollUntil(Func<HttpResponseMessage, bool> condition, Option<Duration> timeout = default) =>
        PollUntil(hrm => Task.FromResult(condition(hrm)), timeout);

    public Task<HttpResponseMessage> PollUntil(AsyncFunc<HttpResponseMessage, bool> condition, Option<Duration> timeout = default) =>
        PollWhile(async httpRM => !await condition(httpRM), timeout);

    public async Task IgnoringResponse() => await AsHttpResponseMessage();

    public async Task<VerifiableHttpResponse<T>> AsVerifiableResponse<T>()
    {
        var (response, content) = await AsResponseAndContent<T>();
        return new(content, response.StatusCode);
    }

    public async Task<VerifiableHttpResponse<T>> AsVerifiableErrorResponse<T>()
    {
        var (response, content) = await AsResponseAndContent<T>(expectSuccess: false);
        return new(content, response.StatusCode);
    }

    public async Task<ResponseDto<T>> AsContentOnly<T>()
    {
        var (_, content) = await AsResponseAndContent<T>();
        return content;
    }

    public async Task<T> AsDataOnly<T>()
    {
        var content = await AsContentOnly<T>();
        return content.Data;
    }

    public async Task<(HttpResponseMessage Response, ResponseDto<T> Content)> AsResponseAndContent<T>(bool expectSuccess = true)
    {
        var response = await AsHttpResponseMessage();
        if (expectSuccess && (!response.IsSuccessStatusCode || response.Content is null))
        {
            throw await HttpRequestUnexpectedFailureException.Create(response);
        }
        var content = await ParseContent<T>(response);
        return (response, content);
    }

    private async Task<ResponseDto<T>> ParseContent<T>(HttpResponseMessage response)
    {
        var bodyAsJson = await response.Content.ReadAsStringAsync();
        try
        {
            return JsonConvert.DeserializeObject<ResponseDto<T>>(bodyAsJson, _settings);
        }
        catch (JsonException e)
        {
            throw new Exception($"Failed to parse response as {typeof(T).Name}. Content was:\n\n{bodyAsJson}", e);
        }
    }

    private static async Task<HttpRequestMessage> Clone(HttpRequestMessage httpRequestMessage)
    {
        HttpRequestMessage httpRequestMessageClone = new HttpRequestMessage(httpRequestMessage.Method, httpRequestMessage.RequestUri);

        if (httpRequestMessage.Content != null)
        {
            var ms = new MemoryStream();
            await httpRequestMessage.Content.CopyToAsync(ms);
            ms.Position = 0;
            httpRequestMessageClone.Content = new StreamContent(ms);

            httpRequestMessage.Content.Headers?.ToList().ForEach(header => httpRequestMessageClone.Content.Headers.Add(header.Key, header.Value));
        }

        httpRequestMessageClone.Version = httpRequestMessage.Version;

        httpRequestMessage.Options.ToList().ForEach(props => httpRequestMessageClone.Options.TryAdd(props.Key, props.Value));
        httpRequestMessage.Headers.ToList().ForEach(header => httpRequestMessageClone.Headers.TryAddWithoutValidation(header.Key, header.Value));

        return httpRequestMessageClone;
    }
}

public class HttpRequestUnexpectedFailureException : Exception
{
    private HttpRequestUnexpectedFailureException(string message) : base(message)
    {
    }

    public static async Task<HttpRequestUnexpectedFailureException> Create(HttpResponseMessage response) => new(
        $$"""
        HttpRequest failed unexpectedly.
        Response
        {
        StatusCode: {{response.StatusCode}}
        Headers:
        {{response.Headers}}
        Content:
        {{await ReadContent(response.Content)}}
        }
        
        Request
        {
        Method: {{response.RequestMessage.Method}}
        Uri: {{response.RequestMessage.RequestUri}}
        Headers:
        {{response.RequestMessage.Headers}}
        Content:
        {{await ReadContent(response.RequestMessage.Content)}}
        }
        """);

    private static async Task<string> ReadContent(HttpContent content) =>
        await content.AsOption().MapAsync(c => c.ReadAsStringAsync()) | string.Empty;
}
