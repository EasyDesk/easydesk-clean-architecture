using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Web.Versioning;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using NodaTime;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public abstract class HttpRequestBuilder
{
    public abstract HttpRequestBuilder Headers(Func<ImmutableHttpHeaders, ImmutableHttpHeaders> configureHeaders);

    public abstract HttpRequestBuilder WithApiVersion(ApiVersion version);

    public abstract HttpRequestBuilder Tenant(TenantId tenantId);

    public abstract HttpRequestBuilder NoTenant();

    public abstract HttpRequestBuilder AuthenticateAs(Agent agent);

    public abstract HttpRequestBuilder NoAuthentication();

    public abstract HttpRequestBuilder WithContent(ImmutableHttpContent content);

    public abstract HttpRequestBuilder WithQuery(string key, string value);

    public abstract HttpRequestBuilder WithoutQuery(string key);

    public abstract HttpRequestBuilder WithTimeout(Duration timeout);
}

public class HttpRequestBuilder<B> : HttpRequestBuilder
    where B : HttpRequestBuilder<B>
{
    private static readonly Duration _defaultTimeout = Duration.FromSeconds(30);

    private readonly string _endpoint;
    private readonly HttpMethod _method;
    private readonly ITestHttpAuthentication _testHttpAuthentication;
    private readonly IList<Func<ImmutableHttpRequestMessage, ImmutableHttpRequestMessage>> _configureRequest = new List<Func<ImmutableHttpRequestMessage, ImmutableHttpRequestMessage>>();
    private readonly Dictionary<string, StringValues> _queryParameters = new();

    public HttpRequestBuilder(
        string endpoint,
        HttpMethod method,
        ITestHttpAuthentication testHttpAuthentication)
    {
        _endpoint = endpoint;
        _method = method;
        _testHttpAuthentication = testHttpAuthentication;
    }

    protected IImmutableDictionary<string, IImmutableList<string>> Query =>
        _queryParameters.ToImmutableDictionary(pair => pair.Key, pair => pair.Value.ToImmutableList() as IImmutableList<string>);

    protected Duration Timeout { get; private set; } = _defaultTimeout;

    public override B WithApiVersion(ApiVersion version) =>
        Headers(h => h.Replace(RestApiVersioning.VersionHeader, version.ToString()));

    public override B Tenant(TenantId tenantId) =>
        Headers(h => h.Replace(CommonTenantReaders.TenantIdHttpHeader, tenantId));

    public override B NoTenant() =>
        Headers(h => h.Remove(CommonTenantReaders.TenantIdHttpHeader));

    public override B Headers(Func<ImmutableHttpHeaders, ImmutableHttpHeaders> configureHeaders) =>
        ConfigureRequest(r => r with { Headers = configureHeaders(r.Headers) });

    public override B AuthenticateAs(Agent agent) =>
        ConfigureRequest(r => _testHttpAuthentication.ConfigureAuthentication(r, agent));

    public override B NoAuthentication() => ConfigureRequest(_testHttpAuthentication.RemoveAuthentication);

    public override B WithContent(ImmutableHttpContent? content) => ConfigureRequest(r => r with { Content = content });

    public override B WithQuery(string key, string value) =>
        ConfigureQuery(q => q[key] = value);

    public override B WithoutQuery(string key) =>
        ConfigureQuery(q => q.Remove(key));

    private B ConfigureRequest(Func<ImmutableHttpRequestMessage, ImmutableHttpRequestMessage> configure)
    {
        _configureRequest.Add(configure);
        return (B)this;
    }

    private B ConfigureQuery(Action<Dictionary<string, StringValues>> configure)
    {
        configure(_queryParameters);
        return (B)this;
    }

    public override B WithTimeout(Duration timeout)
    {
        Timeout = timeout;
        return (B)this;
    }

    protected ImmutableHttpRequestMessage CreateRequest()
    {
        var uri = QueryHelpers.AddQueryString(_endpoint, _queryParameters);
        var request = new ImmutableHttpRequestMessage(_method, uri);
        foreach (var f in _configureRequest)
        {
            request = f(request);
        }
        return request;
    }
}
