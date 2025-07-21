using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.Commons.Collections;
using System.Net.Mime;
using System.Text.Json;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public class HttpTestHelper
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IEnumerable<IHttpRequestConfigurator> _requestConfigurators;
    private readonly ITestHttpAuthentication _httpAuthentication;

    public HttpTestHelper(
        HttpClient httpClient,
        JsonOptionsConfigurator jsonOptionsConfigurator,
        IEnumerable<IHttpRequestConfigurator> requestConfigurators,
        ITestHttpAuthentication? httpAuthentication = null)
    {
        _httpClient = httpClient;
        _jsonOptions = jsonOptionsConfigurator.CreateOptions();
        _requestConfigurators = requestConfigurators;
        _httpAuthentication = httpAuthentication ?? new NoAuthentication();
    }

    public HttpSingleRequestExecutor<R> Get<R>(string requestUri) =>
        Request<R>(requestUri, HttpMethod.Get);

    public HttpPaginatedRequestExecutor<R> GetPaginated<R>(string requestUri) =>
        RequestPaginated<R>(requestUri, HttpMethod.Get);

    public HttpSingleRequestExecutor<R> Post<T, R>(string requestUri, T body) =>
        Request<R>(requestUri, HttpMethod.Post, JsonContent(body));

    public HttpSingleRequestExecutor<R> Put<T, R>(string requestUri, T body) =>
        Request<R>(requestUri, HttpMethod.Put, JsonContent(body));

    public HttpSingleRequestExecutor<R> Delete<R>(string requestUri) =>
        Request<R>(requestUri, HttpMethod.Delete);

    private ImmutableHttpContent JsonContent<T>(T body)
    {
        var bodyAsJson = JsonSerializer.Serialize(body, _jsonOptions);
        var content = new ImmutableHttpContent(bodyAsJson, MediaTypeNames.Application.Json);
        return content;
    }

    public HttpSingleRequestExecutor<R> Request<R>(string requestUri, HttpMethod method, ImmutableHttpContent? content = null)
    {
        var builder = new HttpRequestBuilder(requestUri, method, _httpAuthentication)
            .Content(content)
            .Also(ConfigureRequest);
        return new(builder, _httpClient, _jsonOptions);
    }

    private HttpPaginatedRequestExecutor<R> RequestPaginated<R>(string requestUri, HttpMethod method, ImmutableHttpContent? content = null)
    {
        var builder = new HttpRequestBuilder(requestUri, method, _httpAuthentication)
            .Content(content)
            .Also(ConfigureRequest);
        return new(builder, _httpClient, _jsonOptions);
    }

    private void ConfigureRequest(HttpRequestBuilder request)
    {
        _requestConfigurators.ForEach(c => c.ConfigureHttpRequest(request));
    }
}
