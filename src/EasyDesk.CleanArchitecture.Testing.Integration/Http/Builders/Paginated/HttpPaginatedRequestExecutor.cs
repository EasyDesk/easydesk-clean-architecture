using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Tools.Collections;
using Newtonsoft.Json;
using NodaTime;
using System.Runtime.CompilerServices;
using System.Web;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;

public class HttpPaginatedRequestExecutor<T>
    : HttpRequestExecutor<HttpPaginatedResponsesWrapper<T>, IEnumerable<HttpPaginatedResponseWrapper<T>>>
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public HttpPaginatedRequestExecutor(
        HttpRequestMessage httpRequestMessage,
        HttpClient httpClient,
        JsonSerializerSettings jsonSerializerSettings,
        ITestHttpAuthentication testHttpAuthentication)
        : base(httpRequestMessage, testHttpAuthentication)
    {
        _httpClient = httpClient;
        _jsonSerializerSettings = jsonSerializerSettings;
    }

    public HttpPaginatedResponsesWrapper<T> PollUntil(Func<IEnumerable<T>, bool> predicate, Option<Duration> timeout = default) =>
        PollUntil(async httpRM => predicate(await httpRM.AsVerifiableEnumerable()), timeout);

    public HttpPaginatedResponsesWrapper<T> PollWhile(Func<IEnumerable<T>, bool> predicate, Option<Duration> timeout = default) =>
        PollWhile(async httpRM => predicate(await httpRM.AsVerifiableEnumerable()), timeout);

    private HttpPaginatedResponseWrapper<T> Wrap(AsyncFunc<HttpResponseMessage> message) =>
        new(message, _jsonSerializerSettings);

    private async IAsyncEnumerable<HttpPaginatedResponseWrapper<T>> CollectPagesSequentially(
        HttpRequestMessage request,
        [EnumeratorCancellation] CancellationToken requestToken)
    {
        var page = Wrap(() => _httpClient.SendAsync(request, requestToken));
        var (pageIndex, pageCount) = await page.PageIndexAndCount();
        yield return page;
        for (var i = pageIndex + 1; i < pageCount; i++)
        {
            var req = await request.Clone();
            SetPage(req, i);
            page = Wrap(() => _httpClient.SendAsync(req, requestToken));
            await page.EnsureSuccess();
            yield return page;
        }
    }

    private HttpRequestMessage SetPage(HttpRequestMessage req, int pageIndex)
    {
        var query = HttpUtility.ParseQueryString(req.RequestUri.Query);
        query[nameof(PaginationDto.PageIndex)] = pageIndex.ToString();
        var uriBuilder = new UriBuilder(req.RequestUri);
        uriBuilder.Query = query.ToString();
        req.RequestUri = uriBuilder.Uri;
        return req;
    }

    protected override Task<IEnumerable<HttpPaginatedResponseWrapper<T>>> Send(HttpRequestMessage request, CancellationToken cancellationToken) =>
        CollectPagesSequentially(request, cancellationToken)
        .ToEnumerableAsync();

    protected override HttpPaginatedResponsesWrapper<T> Wrap(AsyncFunc<IEnumerable<HttpPaginatedResponseWrapper<T>>> request) =>
        new(request);
}
