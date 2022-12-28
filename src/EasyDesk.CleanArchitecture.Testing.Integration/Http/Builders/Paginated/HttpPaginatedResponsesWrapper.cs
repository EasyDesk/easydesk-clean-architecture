using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;

public class HttpPaginatedResponsesWrapper<T> : ResponseCache<IEnumerable<HttpPaginatedResponseWrapper<T>>>
{
    private readonly AsyncFunc<IEnumerable<HttpPaginatedResponseWrapper<T>>> _responses;

    public HttpPaginatedResponsesWrapper(AsyncFunc<IEnumerable<HttpPaginatedResponseWrapper<T>>> responses)
    {
        _responses = responses;
    }

    protected override async Task<IEnumerable<HttpPaginatedResponseWrapper<T>>> Fetch() => await _responses();

    public async Task<IEnumerable<T>> AsVerifiableEnumerable()
    {
        var result = new List<T>();
        foreach (var response in await Response)
        {
            result.AddRange(await response.AsData());
        }
        return result;
    }

    public async Task EnsureSuccess()
    {
        foreach (var response in await Response)
        {
            await response.EnsureSuccess();
        }
    }
}
