using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.CleanArchitecture.Web.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using NodaTime;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;
public class HttpRequestBuilder
{
    private readonly HttpRequestMessage _request;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _settings;
    private readonly IClock _clock;
    private readonly Option<JwtTokenConfiguration> _jwtConfiguration;

    public HttpRequestBuilder(
        HttpRequestMessage request,
        HttpClient httpClient,
        IClock clock,
        JsonSerializerSettings settings,
        Option<JwtTokenConfiguration> jwtAuth)
    {
        _request = request;
        _httpClient = httpClient;
        _clock = clock;
        _settings = settings;
        _jwtConfiguration = jwtAuth;
    }

    public HttpRequestBuilder Headers(Action<HttpRequestHeaders> configureHeaders)
    {
        configureHeaders(_request.Headers);
        return this;
    }

    private void ReplaceHeader(HttpRequestHeaders headers, string name, string value)
    {
        headers.Remove(name);
        headers.Add(name, value);
    }

    public HttpRequestBuilder WithApiVersion(ApiVersion version) =>
        Headers(h => ReplaceHeader(h, ApiVersioningUtils.VersionHeader, version.ToString()));

    public HttpRequestBuilder Tenant(string tenantId) =>
        Headers(h => ReplaceHeader(h, MultitenancyDefaults.TenantIdHttpHeader, tenantId));

    public HttpRequestBuilder NoTenant() =>
        Headers(h => h.Remove(MultitenancyDefaults.TenantIdHttpHeader));

    public HttpRequestBuilder NoAuthentication() =>
        Headers(h => h.Remove(HeaderNames.Authorization));

    private string ForgeJwt(string userId)
    {
        return new JwtFacade(_clock)
        .Create(
            new Claim[]
            {
                new(ClaimTypes.NameIdentifier, userId)
            },
            _jwtConfiguration.OrElseThrow(() =>
                    new InvalidOperationException(
                        "JwtTokenConfiguration must be provided in order to forge valid JWTs.")));
    }

    public HttpRequestBuilder AuthenticateWithJwtAs(string userId) =>
        Headers(h => ReplaceHeader(h, HeaderNames.Authorization, "Bearer " + ForgeJwt(userId)));

    public async Task<HttpResponseMessage> AsHttpResponseMessage() =>
        await _httpClient.SendAsync(_request);

    public async Task IgnoringResponse() => await AsHttpResponseMessage();

    public async Task<VerifiableHttpResponse<T>> AsVerifiableResponse<T>()
    {
        var (response, content) = await AsResponseAndContent<T>();
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

    public async Task<(HttpResponseMessage Response, ResponseDto<T> Content)> AsResponseAndContent<T>()
    {
        var response = await AsHttpResponseMessage();
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
}
