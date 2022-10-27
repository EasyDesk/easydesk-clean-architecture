using EasyDesk.CleanArchitecture.Application.Json;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Mime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public class HttpTestHelper
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _settings;

    public HttpTestHelper(HttpClient httpClient, JsonSettingsConfigurator jsonSettingsConfigurator)
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
        return new(request, _httpClient, _settings);
    }

    public HttpTestHelper WithDefaultHeaders(Action<HttpRequestHeaders> configureHeaders)
    {
        configureHeaders(_httpClient.DefaultRequestHeaders);
        return this;
    }
}
