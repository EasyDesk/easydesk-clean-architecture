using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Tasks;
using NodaTime;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;

public sealed class HttpPaginatedRequestExecutor<T> :
    HttpRequestExecutor<HttpPageSequenceWrapper<T>, IEnumerable<HttpResponseWrapper<T, PaginationMetaDto>>, HttpPaginatedRequestExecutor<T>>
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public HttpPaginatedRequestExecutor(
        string endpoint,
        HttpMethod method,
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions,
        ITestHttpAuthentication testHttpAuthentication)
        : base(endpoint, method, testHttpAuthentication)
    {
        _httpClient = httpClient;
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    private HttpResponseWrapper<T, PaginationMetaDto> WrapSinglePage(AsyncFunc<ImmutableHttpResponseMessage> message) =>
        new(message, _jsonSerializerOptions);

    protected override Task<IEnumerable<HttpResponseWrapper<T, PaginationMetaDto>>> MakeRequest(CancellationToken timeoutToken) =>
        EnumeratePages(timeoutToken).ToEnumerableAsync();

    private async IAsyncEnumerable<HttpResponseWrapper<T, PaginationMetaDto>> EnumeratePages([EnumeratorCancellation] CancellationToken timeoutToken)
    {
        bool hasNextPage;
        var initialPageOption = PageIndex;
        do
        {
            timeoutToken.ThrowIfCancellationRequested();
            var page = GetSinglePage(timeoutToken);
            await page.EnsureSuccess();
            var paginationMetadata = await page.AsMetadata();
            var count = paginationMetadata.Count;
            var pageSize = paginationMetadata.PageSize;
            var pageIndex = paginationMetadata.PageIndex;
            hasNextPage = count >= pageSize;
            yield return page;
            this.SetPageIndex(pageIndex + 1);
        }
        while (hasNextPage);
        initialPageOption.Match(some: i => this.SetPageIndex(i), none: () => this.RemovePageIndex());
    }

    public HttpResponseWrapper<T, PaginationMetaDto> SinglePage(int? pageIndex = null, Duration? timeout = null)
    {
        pageIndex.AsOption().IfPresent(i => this.SetPageIndex(i));
        var actualTimeout = timeout ?? Timeout;
        using var cts = new CancellationTokenSource(actualTimeout.ToTimeSpan());
        return GetSinglePage(cts.Token);
    }

    private HttpResponseWrapper<T, PaginationMetaDto> GetSinglePage(CancellationToken timeoutToken)
    {
        var request = CreateRequest();
        var page = WrapSinglePage(async () =>
        {
            using var req = request.ToHttpRequestMessage();
            using var res = await _httpClient.SendAsync(req, timeoutToken);
            return await ImmutableHttpResponseMessage.From(res);
        });
        return page;
    }

    protected override HttpPageSequenceWrapper<T> Wrap(AsyncFunc<IEnumerable<HttpResponseWrapper<T, PaginationMetaDto>>> request) =>
        new(request);

    private Option<int> PageIndex => Query.Get(nameof(PaginationDto.PageIndex)).FlatMap(p => TryOption<string, int>(int.TryParse, p[0]));
}

public static class HttpPaginatedRequestExecutorExtensions
{
    public static HttpPageSequenceWrapper<IEnumerable<T>> PollUntil<T>(this HttpPaginatedRequestExecutor<IEnumerable<T>> executor, Func<IEnumerable<T>, bool> predicate, Duration? interval = null, Duration? timeout = null) =>
        executor.PollUntil(async wrapped => predicate(await wrapped.AsVerifiableEnumerable()), interval, timeout);

    public static HttpPageSequenceWrapper<IEnumerable<T>> PollWhile<T>(this HttpPaginatedRequestExecutor<IEnumerable<T>> executor, Func<IEnumerable<T>, bool> predicate, Duration? interval = null, Duration? timeout = null) =>
        executor.PollWhile(async wrapped => predicate(await wrapped.AsVerifiableEnumerable()), interval, timeout);
}
