using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Web.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

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

public abstract class HttpRequestBuilder<T, E> : HttpRequestBuilder
    where E : HttpRequestBuilder<T, E>
{
    private readonly HttpRequestMessage _request;
    private readonly ITestHttpAuthentication _testHttpAuthentication;

    public HttpRequestBuilder(
        HttpRequestMessage request,
        ITestHttpAuthentication testHttpAuthentication)
    {
        _request = request;
        _testHttpAuthentication = testHttpAuthentication;
    }

    public override E WithApiVersion(ApiVersion version) =>
        Headers(h => h.Replace(ApiVersioningUtils.VersionHeader, version.ToString()));

    public override E Tenant(string tenantId) =>
        Headers(h => h.Replace(MultitenancyDefaults.TenantIdHttpHeader, tenantId));

    public override E NoTenant() =>
        Headers(h => h.Remove(MultitenancyDefaults.TenantIdHttpHeader));

    public override E AuthenticateAs(string userId) =>
        Authenticate(new Claim[] { new Claim(ClaimTypes.NameIdentifier, userId) });

    public override E Headers(Action<HttpRequestHeaders> configureHeaders)
    {
        configureHeaders(_request.Headers);
        return (E)this;
    }

    public override E Authenticate(IEnumerable<Claim> identity)
    {
        _testHttpAuthentication.ConfigureAuthentication(_request, identity);
        return (E)this;
    }

    public override E NoAuthentication()
    {
        _testHttpAuthentication.RemoveAuthentication(_request);
        return (E)this;
    }
}
