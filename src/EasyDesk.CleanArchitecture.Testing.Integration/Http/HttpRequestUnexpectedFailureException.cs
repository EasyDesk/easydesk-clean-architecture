namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public class HttpRequestUnexpectedFailureException : Exception
{
    private HttpRequestUnexpectedFailureException(string message) : base(message)
    {
    }

    public static async Task<HttpRequestUnexpectedFailureException> Create(HttpResponseMessage response) => new(
        $$"""
        HttpRequest failed unexpectedly.
        Response
        {
        StatusCode: {{response.StatusCode}}
        Headers:
        {{response.Headers}}
        Content:
        {{await ReadContent(response.Content)}}
        }
        
        Request
        {
        Method: {{response.RequestMessage.Method}}
        Uri: {{response.RequestMessage.RequestUri}}
        Headers:
        {{response.RequestMessage.Headers}}
        Content:
        {{await ReadContent(response.RequestMessage.Content)}}
        }
        """);

    private static async Task<string> ReadContent(HttpContent content) =>
        await content.AsOption().MapAsync(c => c.ReadAsStringAsync()) | string.Empty;
}
