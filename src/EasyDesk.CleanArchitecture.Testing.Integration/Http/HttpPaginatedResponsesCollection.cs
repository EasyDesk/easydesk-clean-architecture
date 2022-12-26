namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public class HttpPaginatedResponsesCollection
{
    private readonly IEnumerable<HttpPaginatedResponseWrapper> _responses;

    public HttpPaginatedResponsesCollection(IEnumerable<HttpPaginatedResponseWrapper> responses)
    {
        _responses = responses;
    }

    public async Task<IEnumerable<T>> AsVerifiable<T>()
    {
        var result = new List<T>();
        foreach (var response in _responses)
        {
            result.AddRange(await response.GetCollection<T>());
        }
        return result;
    }
}

public static partial class HttpResponseBuilderExtensions
{
    public static async Task<IEnumerable<T>> AsVerifiable<T>(this Task<HttpPaginatedResponsesCollection> builder) =>
        await (await builder).AsVerifiable<T>();
}
