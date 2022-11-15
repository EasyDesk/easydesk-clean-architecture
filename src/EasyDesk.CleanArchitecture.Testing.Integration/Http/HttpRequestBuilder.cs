using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.CleanArchitecture.Web.Versioning;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public class HttpRequestBuilder
{
    private readonly HttpRequestMessage _request;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _settings;

    public HttpRequestBuilder(HttpRequestMessage request, HttpClient httpClient, JsonSerializerSettings settings)
    {
        _request = request;
        _httpClient = httpClient;
        _settings = settings;
    }

    public HttpRequestBuilder Headers(Action<HttpRequestHeaders> configureHeaders)
    {
        configureHeaders(_request.Headers);
        return this;
    }

    public HttpRequestBuilder WithApiVersion(ApiVersion version) =>
        Headers(h => h.Add(ApiVersioningUtils.VersionHeader, version.ToString()));

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
