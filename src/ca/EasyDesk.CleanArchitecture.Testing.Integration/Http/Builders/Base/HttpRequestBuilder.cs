using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.Commons.Collections.Immutable;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public class HttpRequestBuilder
{
    private static readonly Duration _defaultTimeout = Duration.FromSeconds(30);

    private readonly string _endpoint;
    private readonly HttpMethod _method;
    private readonly ITestHttpAuthentication _testHttpAuthentication;
    private readonly List<Func<ImmutableHttpRequestMessage, ImmutableHttpRequestMessage>> _configureRequest = [];

    public HttpRequestBuilder(
        string endpoint,
        HttpMethod method,
        ITestHttpAuthentication testHttpAuthentication)
    {
        _endpoint = endpoint;
        _method = method;
        _testHttpAuthentication = testHttpAuthentication;
    }

    public ImmutableHttpQueryParameters QueryParameters { get; private set; } = ImmutableHttpQueryParameters.Empty;

    public Duration RequestTimeout { get; private set; } = _defaultTimeout;

    public HttpRequestBuilder WithApiVersion(ApiVersion version) =>
        Headers(h => h.Replace(RestApiVersioning.VersionHeader, version.ToString()));

    public HttpRequestBuilder Tenant(TenantId tenantId) =>
        Headers(h => h.Replace(CommonTenantReaders.TenantIdHttpHeader, tenantId));

    public HttpRequestBuilder NoTenant() =>
        Headers(h => h.Remove(CommonTenantReaders.TenantIdHttpHeader));

    public HttpRequestBuilder Headers(Func<ImmutableHttpHeaders, ImmutableHttpHeaders> configureHeaders) =>
        ConfigureRequest(r => r with { Headers = configureHeaders(r.Headers) });

    public HttpRequestBuilder Header(string header, params IFixedList<string> value) =>
        Headers(h => h.Replace(header, value));

    public HttpRequestBuilder AuthenticateAs(Agent agent) =>
        ConfigureRequest(r => _testHttpAuthentication.ConfigureAuthentication(r, agent));

    public HttpRequestBuilder NoAuthentication() => ConfigureRequest(_testHttpAuthentication.RemoveAuthentication);

    public HttpRequestBuilder Content(ImmutableHttpContent? content) => ConfigureRequest(r => r with { Content = content });

    public HttpRequestBuilder Query(Func<ImmutableHttpQueryParameters, ImmutableHttpQueryParameters> configure)
    {
        QueryParameters = configure(QueryParameters);
        return this;
    }

    public HttpRequestBuilder Query(string key, params IFixedList<string> value) =>
        Query(x => x.Replace(key, value));

    private HttpRequestBuilder ConfigureRequest(Func<ImmutableHttpRequestMessage, ImmutableHttpRequestMessage> configure)
    {
        _configureRequest.Add(configure);
        return this;
    }

    public HttpRequestBuilder Timeout(Duration timeout)
    {
        RequestTimeout = timeout;
        return this;
    }

    public ImmutableHttpRequestMessage CreateRequest()
    {
        var queryString = QueryParameters.Map
            .Select(x => new KeyValuePair<string, StringValues>(x.Key, new([.. x.Value])));

        var uri = QueryHelpers.AddQueryString(_endpoint, queryString);
        var request = new ImmutableHttpRequestMessage(_method, uri);
        foreach (var f in _configureRequest)
        {
            request = f(request);
        }
        return request;
    }
}
