using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;

public sealed class HttpPageSequenceWrapper<T>
{
    private readonly AsyncCache<IEnumerable<HttpPageResponseWrapper<T>>> _response;

    public HttpPageSequenceWrapper(AsyncFunc<IEnumerable<HttpPageResponseWrapper<T>>> responses)
    {
        _response = new(responses);
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

    private async Task<IEnumerable<HttpPageResponseWrapper<T>>> GetResponse() => await _response.Get();
}
