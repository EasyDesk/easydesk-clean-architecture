using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Web.Versioning;
using Microsoft.AspNetCore.Mvc;
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
}

public abstract class HttpRequestBuilder<B> : HttpRequestBuilder
    where B : HttpRequestBuilder<B>
{
    private readonly ITestHttpAuthentication _testHttpAuthentication;
    private readonly Func<HttpRequestMessage> _requestFactory;
    private Action<HttpRequestMessage> _configureRequest;

    public HttpRequestBuilder(
        Func<HttpRequestMessage> requestFactory,
        ITestHttpAuthentication testHttpAuthentication)
    {
        _requestFactory = requestFactory;
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

    private B ConfigureRequest(Action<HttpRequestMessage> configure)
    {
        _configureRequest += configure;
        return (B)this;
    }

    protected HttpRequestMessage CreateRequest()
    {
        var request = _requestFactory();
        _configureRequest?.Invoke(request);
        return request;
    }
}
