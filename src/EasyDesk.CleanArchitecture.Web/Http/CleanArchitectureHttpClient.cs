using EasyDesk.CleanArchitecture.Application.Json;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Mime;

namespace EasyDesk.CleanArchitecture.Web.Http;

public class CleanArchitectureHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _settings;

    public CleanArchitectureHttpClient(HttpClient httpClient, JsonSettingsConfigurator jsonSettingsConfigurator)
    {
        _httpClient = httpClient;
        _settings = jsonSettingsConfigurator.CreateSettings();
    }

    public HttpRequestBuilder Get(string requestUri) =>
        Request(requestUri, HttpMethod.Get);

    public HttpRequestBuilder Post<T>(string requestUri, T body) =>
        Request(requestUri, HttpMethod.Post, JsonContent(body));

    public HttpRequestBuilder Put<T>(string requestUri, T body) =>
        Request(requestUri, HttpMethod.Put, JsonContent(body));

    public HttpRequestBuilder Delete(string requestUri) =>
        Request(requestUri, HttpMethod.Delete);

    private StringContent JsonContent<T>(T body)
    {
        var bodyAsJson = JsonConvert.SerializeObject(body, Formatting.None, _settings);
        var content = new StringContent(bodyAsJson, null, MediaTypeNames.Application.Json);
        return content;
    }

    private HttpRequestBuilder Request(string requestUri, HttpMethod method, HttpContent content = null)
    {
        var request = new HttpRequestMessage(method, requestUri)
        {
            Content = content
        };
        return new(_httpClient, request);
    }

    public CleanArchitectureHttpClient WithDefaultHeaders(Action<HttpRequestHeaders> configureHeaders)
    {
        configureHeaders(_httpClient.DefaultRequestHeaders);
        return this;
    }
}
