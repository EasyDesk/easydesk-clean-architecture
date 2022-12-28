using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Tools.Collections;
using Newtonsoft.Json;
using NodaTime;
using System.Runtime.CompilerServices;
using System.Web;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;

public class HttpPaginatedRequestExecutor<T> :
    HttpRequestExecutor<HttpPaginatedResponsesWrapper<T>, IEnumerable<HttpPaginatedResponseWrapper<T>>>
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

    public HttpPaginatedResponsesWrapper<T> PollUntil(Func<IEnumerable<T>, bool> predicate, Duration? interval = null, Duration? timeout = null) =>
        PollUntil(async httpRM => predicate(await httpRM.AsVerifiableEnumerable()), interval, timeout);

    public HttpPaginatedResponsesWrapper<T> PollWhile(Func<IEnumerable<T>, bool> predicate, Duration? interval = null, Duration? timeout = null) =>
        PollWhile(async httpRM => predicate(await httpRM.AsVerifiableEnumerable()), interval, timeout);

    private HttpPaginatedResponseWrapper<T> Wrap(AsyncFunc<HttpResponseMessage> message) =>
        new(message, _jsonSerializerSettings);

    private async IAsyncEnumerable<HttpPaginatedResponseWrapper<T>> CollectPagesSequentially(
        [EnumeratorCancellation] CancellationToken requestToken)
    {
        var hasNextPage = true;
        var pageIndex = 0;
        do
        {
            var request = CreateRequest();
            SetPageIndex(request, pageIndex);
            var page = Wrap(() => _httpClient.SendAsync(request, requestToken));
            var (_, pageCount) = await page.PageIndexAndCount();
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

    protected override Task<IEnumerable<HttpPaginatedResponseWrapper<T>>> Send(CancellationToken cancellationToken) =>
        CollectPagesSequentially(cancellationToken).ToEnumerableAsync();

    protected override HttpPaginatedResponsesWrapper<T> Wrap(AsyncFunc<IEnumerable<HttpPaginatedResponseWrapper<T>>> request) =>
        new(request);
}
