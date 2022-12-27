namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public class HttpPaginatedResponsesWrapper<T>
{
    private readonly IAsyncEnumerable<HttpPaginatedResponseWrapper<T>> _responses;

    public HttpPaginatedResponsesWrapper(IAsyncEnumerable<HttpPaginatedResponseWrapper<T>> responses)
    {
        _responses = responses;
    }

    public async Task<IEnumerable<T>> AsVerifiable()
    {
        var result = new List<T>();
        await foreach (var response in _responses)
        {
            result.AddRange(await response.AsData());
        }
        return result;
    }

    public async Task EnsureSuccess()
    {
        await foreach (var response in _responses)
        {
            await response.EnsureSuccess();
        }
    }
}
