using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Web.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public abstract class HttpRequestBuilder
{
    public abstract HttpRequestBuilder Headers(Action<HttpRequestHeaders> configureHeaders);

    public abstract HttpRequestBuilder WithApiVersion(ApiVersion version);

    public abstract HttpRequestBuilder Tenant(string tenantId);

    public abstract HttpRequestBuilder NoTenant();

    public abstract HttpRequestBuilder AuthenticateAs(string userId);

    public abstract HttpRequestBuilder Authenticate(IEnumerable<Claim> identity);

    public abstract HttpRequestBuilder NoAuthentication();

    public abstract HttpRequestBuilder WithContent(HttpContent content);

    public abstract HttpRequestBuilder WithQuery(string key, string value);

    public abstract HttpRequestBuilder WithoutQuery(string key);
}

public class HttpRequestBuilder<B> : HttpRequestBuilder
    where B : HttpRequestBuilder<B>
{
    private readonly string _endpoint;
    private readonly HttpMethod _method;
    private readonly ITestHttpAuthentication _testHttpAuthentication;
    private Action<HttpRequestMessage> _configureRequest;
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

    public override B WithApiVersion(ApiVersion version) =>
        Headers(h => h.Replace(ApiVersioningUtils.VersionHeader, version.ToString()));

    public override B Tenant(string tenantId) =>
        Headers(h => h.Replace(MultitenancyDefaults.TenantIdHttpHeader, tenantId));

    public override B NoTenant() =>
        Headers(h => h.Remove(MultitenancyDefaults.TenantIdHttpHeader));

    public override B AuthenticateAs(string userId) =>
        Authenticate(new Claim[] { new Claim(ClaimTypes.NameIdentifier, userId) });

    public override B Headers(Action<HttpRequestHeaders> configureHeaders) =>
        ConfigureRequest(r => configureHeaders(r.Headers));

    public override B Authenticate(IEnumerable<Claim> identity) =>
        ConfigureRequest(r => _testHttpAuthentication.ConfigureAuthentication(r, identity));

    public override B NoAuthentication() => ConfigureRequest(_testHttpAuthentication.RemoveAuthentication);

    public override B WithContent(HttpContent content) => ConfigureRequest(r => r.Content = content);

    public override HttpRequestBuilder WithQuery(string key, string value) =>
        ConfigureQuery(q => q[key] = value);

    public override HttpRequestBuilder WithoutQuery(string key) =>
        ConfigureQuery(q => q.Remove(key));

    private B ConfigureRequest(Action<HttpRequestMessage> configure)
    {
        _configureRequest += configure;
        return (B)this;
    }

    private B ConfigureQuery(Action<Dictionary<string, StringValues>> configure)
    {
        configure(_queryParameters);
        return (B)this;
    }

    protected HttpRequestMessage CreateRequest()
    {
        var uri = QueryHelpers.AddQueryString(_endpoint, _queryParameters);
        var request = new HttpRequestMessage(_method, uri);
        _configureRequest?.Invoke(request);
        return request;
    }
}
