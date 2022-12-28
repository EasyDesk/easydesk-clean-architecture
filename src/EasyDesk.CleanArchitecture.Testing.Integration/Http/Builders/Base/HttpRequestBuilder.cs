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
    private readonly HttpRequestMessage _request;

    public HttpRequestBuilder(
        HttpRequestMessage request,
        ITestHttpAuthentication testHttpAuthentication)
    {
        _request = request;
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

    public override B Headers(Action<HttpRequestHeaders> configureHeaders)
    {
        configureHeaders(_request.Headers);
        return (B)this;
    }

    public override B Authenticate(IEnumerable<Claim> identity)
    {
        _testHttpAuthentication.ConfigureAuthentication(_request, identity);
        return (B)this;
    }

    public override B NoAuthentication()
    {
        _testHttpAuthentication.RemoveAuthentication(_request);
        return (B)this;
    }

    public Task<HttpRequestMessage> Request => _request.Clone();
}
