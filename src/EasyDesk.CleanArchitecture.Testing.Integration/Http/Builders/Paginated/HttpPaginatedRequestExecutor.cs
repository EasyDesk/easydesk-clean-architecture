using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Tools.Collections;
using Newtonsoft.Json;
using NodaTime;
using System.Web;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;

public class HttpPaginatedRequestExecutor<T> :
    HttpRequestExecutor<HttpPageSequenceWrapper<T>, IEnumerable<HttpPageResponseWrapper<T>>>
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public HttpPaginatedRequestExecutor(
        Func<HttpRequestMessage> requestFactory,
        HttpClient httpClient,
        JsonSerializerSettings jsonSerializerSettings,
        ITestHttpAuthentication testHttpAuthentication)
        : base(requestFactory, testHttpAuthentication)
    {
        _httpClient = httpClient;
        _jsonSerializerSettings = jsonSerializerSettings;
    }

    public HttpPageSequenceWrapper<T> PollUntil(Func<IEnumerable<T>, bool> predicate, Duration? interval = null, Duration? timeout = null) =>
        PollUntil(async httpRM => predicate(await httpRM.AsVerifiableEnumerable()), interval, timeout);

    public HttpPageSequenceWrapper<T> PollWhile(Func<IEnumerable<T>, bool> predicate, Duration? interval = null, Duration? timeout = null) =>
        PollWhile(async httpRM => predicate(await httpRM.AsVerifiableEnumerable()), interval, timeout);

    private HttpPageResponseWrapper<T> WrapSinglePage(AsyncFunc<HttpResponseMessage> message) =>
        new(message, _jsonSerializerSettings);

    protected override Task<IEnumerable<HttpPageResponseWrapper<T>>> MakeRequest() =>
        EnumeratePages().ToEnumerableAsync();

    private async IAsyncEnumerable<HttpPageResponseWrapper<T>> EnumeratePages()
    {
        var hasNextPage = true;
        var pageIndex = 0;
        do
        {
            var request = CreateRequest();
            SetPageIndex(request, pageIndex);
            var page = WrapSinglePage(() => _httpClient.SendAsync(request));
            var pageCount = await page.PageCount();
            await page.EnsureSuccess();
            yield return page;
            pageIndex++;
            hasNextPage = pageIndex < pageCount;
        }
        while (hasNextPage);
    }

    private void SetPageIndex(HttpRequestMessage req, int pageIndex)
    {
        var absoluteUri = new Uri(_httpClient.BaseAddress, req.RequestUri);
        var query = HttpUtility.ParseQueryString(absoluteUri.Query);
        query[nameof(PaginationDto.PageIndex)] = pageIndex.ToString();
        var uriBuilder = new UriBuilder(absoluteUri)
        {
            Query = query.ToString()
        };
        req.RequestUri = uriBuilder.Uri;
    }

    protected override HttpPageSequenceWrapper<T> Wrap(AsyncFunc<IEnumerable<HttpPageResponseWrapper<T>>> request) =>
        new(request);
}
