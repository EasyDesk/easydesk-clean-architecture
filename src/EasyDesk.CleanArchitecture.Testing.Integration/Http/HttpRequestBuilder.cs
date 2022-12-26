using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Web.Versioning;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public class HttpRequestBuilder
{
    private readonly HttpRequestMessage _request;
    private readonly HttpClient _httpClient;
    private readonly ITestHttpAuthentication _testHttpAuthentication;
    private readonly JsonSerializerSettings _settings;

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

    public HttpRequestExecutor Build() => new(_httpClient, _request, _settings);
}
