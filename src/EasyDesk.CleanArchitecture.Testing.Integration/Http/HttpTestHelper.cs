using EasyDesk.CleanArchitecture.Application.Json;
using Newtonsoft.Json;
using System.Net.Mime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public class HttpTestHelper
{
    private readonly HttpClient _httpClient;
    private readonly Action<HttpRequestBuilder> _configureRequest;
    private readonly JsonSerializerSettings _settings;

    public HttpTestHelper(
        HttpClient httpClient,
        JsonSettingsConfigurator jsonSettingsConfigurator,
        Action<HttpRequestBuilder> configureRequest = null)
    {
        _httpClient = httpClient;
        _configureRequest = configureRequest;
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
        var builder = new HttpRequestBuilder(request, _httpClient, _settings);
        _configureRequest?.Invoke(builder);
        return builder;
    }
}
