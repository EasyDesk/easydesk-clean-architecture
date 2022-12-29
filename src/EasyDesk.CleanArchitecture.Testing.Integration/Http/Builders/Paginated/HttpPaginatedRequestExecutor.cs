using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Tools.Collections;
using Newtonsoft.Json;
using NodaTime;
using System.Runtime.CompilerServices;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;

public class HttpPaginatedRequestExecutor<T> :
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

    private HttpPageResponseWrapper<T> WrapSinglePage(AsyncFunc<HttpResponseMessage> message) =>
        new(message, _jsonSerializerSettings);

    protected override Task<IEnumerable<HttpPageResponseWrapper<T>>> MakeRequest(CancellationToken timeoutToken) =>
        EnumeratePages(timeoutToken).ToEnumerableAsync();

    private async IAsyncEnumerable<HttpPageResponseWrapper<T>> EnumeratePages([EnumeratorCancellation] CancellationToken timeoutToken)
    {
        var hasNextPage = true;
        var pageIndex = 0;
        do
        {
            timeoutToken.ThrowIfCancellationRequested();
            SetPageIndex(pageIndex);
            var request = CreateRequest();
            var page = WrapSinglePage(() => _httpClient.SendAsync(request));
            var pageCount = await page.PageCount();
            await page.EnsureSuccess();
            yield return page;
            await Task.Delay(100, timeoutToken);
            pageIndex++;
            hasNextPage = pageIndex < pageCount;
        }
        while (hasNextPage);
    }

    private void SetPageIndex(int pageIndex) =>
        WithQuery(nameof(PaginationDto.PageIndex), pageIndex.ToString());

    protected override HttpPageSequenceWrapper<T> Wrap(AsyncFunc<IEnumerable<HttpPageResponseWrapper<T>>> request) =>
        new(request);
}
