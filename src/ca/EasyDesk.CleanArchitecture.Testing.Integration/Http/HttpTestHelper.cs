using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using System.Net.Mime;
using System.Text.Json;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public class HttpTestHelper
{
    private readonly HttpClient _httpClient;
    private readonly ITestHttpAuthentication _httpAuthentication;
    private readonly Action<HttpRequestBuilder>? _configureRequest;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpTestHelper(
        HttpClient httpClient,
        JsonOptionsConfigurator jsonOptionsConfigurator,
        ITestHttpAuthentication? httpAuthentication = null,
        Action<HttpRequestBuilder>? configureRequest = null)
    {
        _httpClient = httpClient;
        _jsonOptions = jsonOptionsConfigurator.CreateOptions();
        _httpAuthentication = httpAuthentication ?? new NoAuthentication();
        _configureRequest = configureRequest;
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
        var builder = new HttpSingleRequestExecutor<R>(requestUri, method, _httpAuthentication, _httpClient, _jsonOptions)
            .WithContent(content);
        _configureRequest?.Invoke(builder);
        return builder;
    }

    private HttpPaginatedRequestExecutor<R> RequestPaginated<R>(string requestUri, HttpMethod method, ImmutableHttpContent? content = null)
    {
        var builder = new HttpPaginatedRequestExecutor<R>(requestUri, method, _httpClient, _jsonOptions, _httpAuthentication)
            .WithContent(content);
        _configureRequest?.Invoke(builder);
        return builder;
    }
}
