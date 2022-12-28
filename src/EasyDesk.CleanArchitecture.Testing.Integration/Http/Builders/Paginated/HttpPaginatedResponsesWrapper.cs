using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;

public class HttpPaginatedResponsesWrapper<T> : ResponseCache<IEnumerable<HttpPaginatedResponseWrapper<T>>>
{
    public HttpPaginatedResponsesWrapper(AsyncFunc<IEnumerable<HttpPaginatedResponseWrapper<T>>> responses)
        : base(responses)
    {
    }

    public async Task<IEnumerable<T>> AsVerifiableEnumerable()
    {
        var result = new List<T>();
        foreach (var response in await GetResponse())
        {
            result.AddRange(await response.AsData());
        }
        return result;
    }

    public async Task EnsureSuccess()
    {
        foreach (var response in await GetResponse())
        {
            await response.EnsureSuccess();
        }
    }
}
