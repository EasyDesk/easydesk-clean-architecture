using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Commons.Collections;
using Newtonsoft.Json;
using NodaTime;
using System.Runtime.CompilerServices;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;

public sealed class HttpPaginatedRequestExecutor<T> :
    HttpRequestExecutor<HttpPageSequenceWrapper<T>, IEnumerable<HttpPageResponseWrapper<T>>, HttpPaginatedRequestExecutor<T>>
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public HttpPaginatedRequestExecutor(
        string endpoint,
        HttpMethod method,
        HttpClient httpClient,
        JsonSerializerSettings jsonSerializerSettings,
        ITestHttpAuthentication testHttpAuthentication)
        : base(endpoint, method, testHttpAuthentication)
    {
        _httpClient = httpClient;
        _jsonSerializerSettings = jsonSerializerSettings;
    }

    public HttpPageSequenceWrapper<T> PollUntil(Func<IEnumerable<T>, bool> predicate, Duration? interval = null, Duration? timeout = null) =>
        PollUntil(async wrapped => predicate(await wrapped.AsVerifiableEnumerable()), interval, timeout);

    public HttpPageSequenceWrapper<T> PollWhile(Func<IEnumerable<T>, bool> predicate, Duration? interval = null, Duration? timeout = null) =>
        PollWhile(async wrapped => predicate(await wrapped.AsVerifiableEnumerable()), interval, timeout);

    private HttpPageResponseWrapper<T> WrapSinglePage(AsyncFunc<ImmutableHttpResponseMessage> message) =>
        new(message, _jsonSerializerSettings);

    protected override Task<IEnumerable<HttpPageResponseWrapper<T>>> MakeRequest(CancellationToken timeoutToken) =>
        EnumeratePages(timeoutToken).ToEnumerableAsync();

    private async IAsyncEnumerable<HttpPageResponseWrapper<T>> EnumeratePages([EnumeratorCancellation] CancellationToken timeoutToken)
    {
        var hasNextPage = true;
        var initialPage = Query.GetOption(nameof(PaginationDto.PageIndex));
        do
        {
            timeoutToken.ThrowIfCancellationRequested();
            var request = CreateRequest();
            var page = WrapSinglePage(async () =>
            {
                using var req = request.ToHttpRequestMessage();
                using var res = await _httpClient.SendAsync(req, timeoutToken);
                return await ImmutableHttpResponseMessage.From(res);
            });
            var paginationMetadata = await page.AsMetadata();
            var pageCount = paginationMetadata.PageCount;
            var pageSize = paginationMetadata.PageSize;
            var pageIndex = paginationMetadata.PageIndex;
            await page.EnsureSuccess();
            hasNextPage = false;
            if (pageCount > 0 && pageSize > 0)
            {
                yield return page;
                hasNextPage = pageIndex < pageCount;
                this.SetPageIndex(pageIndex + 1);
            }
        }
        while (hasNextPage);
        initialPage.Match(some: i => this.SetPageIndex(i[0]), none: () => this.RemovePageIndex());
    }

    protected override HttpPageSequenceWrapper<T> Wrap(AsyncFunc<IEnumerable<HttpPageResponseWrapper<T>>> request) =>
        new(request);
}
