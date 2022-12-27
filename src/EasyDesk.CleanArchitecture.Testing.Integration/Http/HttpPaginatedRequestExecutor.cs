using EasyDesk.CleanArchitecture.Web.Dto;
using Newtonsoft.Json;
using System.Web;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public class HttpPaginatedRequestExecutor<T> : HttpRequestBuilder<T, HttpPaginatedRequestExecutor<T>>
{
    private readonly HttpClient _httpClient;
    private readonly HttpRequestMessage _request;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public HttpPaginatedRequestExecutor(
        HttpRequestMessage httpRequestMessage,
        HttpClient httpClient,
        JsonSerializerSettings jsonSerializerSettings,
        ITestHttpAuthentication testHttpAuthentication)
        : base(httpRequestMessage, testHttpAuthentication)
    {
        _httpClient = httpClient;
        _request = httpRequestMessage;
        _jsonSerializerSettings = jsonSerializerSettings;
    }

    private async IAsyncEnumerable<HttpPaginatedResponseWrapper<T>> CollectPagesSequentially()
    {
        var page = Wrap(() => _httpClient.SendAsync(_request));
        var (pageIndex, pageCount) = await page.PageIndexAndCount();
        yield return page;
        for (int i = pageIndex + 1; i < pageCount; i++)
        {
            var req = await _request.Clone();
            SetPage(req, i);
            page = Wrap(() => _httpClient.SendAsync(req));
            await page.EnsureSuccess();
            yield return page;
        }
    }

    public HttpPaginatedResponsesWrapper<T> CollectEveryPage() =>
        Wrap(CollectPagesSequentially());

    private HttpRequestMessage SetPage(HttpRequestMessage req, int pageIndex)
    {
        var query = HttpUtility.ParseQueryString(req.RequestUri.Query);
        query[nameof(PaginationDto.PageIndex)] = pageIndex.ToString();
        var uriBuilder = new UriBuilder(req.RequestUri);
        uriBuilder.Query = query.ToString();
        req.RequestUri = uriBuilder.Uri;
        return req;
    }

    private HttpPaginatedResponsesWrapper<T> Wrap(IAsyncEnumerable<HttpPaginatedResponseWrapper<T>> message) =>
        new(message);

    private HttpPaginatedResponseWrapper<T> Wrap(AsyncFunc<HttpResponseMessage> message) =>
        new(message, _jsonSerializerSettings);
}
