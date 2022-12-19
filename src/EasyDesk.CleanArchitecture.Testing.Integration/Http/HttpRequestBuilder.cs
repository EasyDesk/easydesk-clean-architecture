using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Web.Dto;
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
    private readonly JsonSerializerSettings _settings;
    private readonly ITestHttpAuthentication _testHttpAuthentication;

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
        if (!response.IsSuccessStatusCode && response.Content is null)
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
