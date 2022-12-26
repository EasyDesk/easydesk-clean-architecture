using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
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

    public HttpRequestBuilder AuthenticateAs(string userId) =>
        Authenticate(new Claim[] { new Claim(ClaimTypes.NameIdentifier, userId) });

    public HttpRequestBuilder Authenticate(IEnumerable<Claim> identity)
    {
        _testHttpAuthentication.ConfigureAuthentication(this, identity);
        return this;
    }

    public HttpRequestBuilder NoAuthentication()
    {
        _testHttpAuthentication.RemoveAuthentication(this);
        return this;
    }

    public async Task<HttpResponseBuilder> Send() =>
        ToBuilder(await _httpClient.SendAsync(_request));

    public Task<HttpResponseBuilder> PollUntil(AsyncFunc<HttpResponseBuilder, bool> condition, Option<Duration> timeout = default) =>
        PollWhile(async httpRM => !await condition(httpRM), timeout);

    public async Task<HttpResponseBuilder> PollWhile(AsyncFunc<HttpResponseBuilder, bool> condition, Option<Duration> timeout = default)
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
        while (await condition(ToBuilder(message)))
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
        return ToBuilder(message);
    }

    private HttpResponseBuilder ToBuilder(HttpResponseMessage message) => new(message, _settings);
}

public static class HttpRequestBuilderExtensions
{
    public static Task<T> PollUntil<T>(this HttpRequestBuilder builder, Func<T, bool> condition) =>
        builder.PollUntil(r => r.Check(condition)).AsData<T>();
}
