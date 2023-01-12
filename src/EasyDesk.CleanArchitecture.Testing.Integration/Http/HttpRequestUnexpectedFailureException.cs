using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public class HttpRequestUnexpectedFailureException : Exception
{
    private HttpRequestUnexpectedFailureException(string message) : base(message)
    {
    }

    public static HttpRequestUnexpectedFailureException Create(ImmutableHttpResponseMessage response) => new(
        $$"""
        HttpRequest failed unexpectedly.
        Response
        {
        StatusCode: {{response.StatusCode}}
        Headers:
        {{response.Headers}}
        Content:
        {{response.Content}}
        }
        
        Request
        {
        Method: {{response.RequestMessage.Method}}
        Uri: {{response.RequestMessage.RequestUri}}
        Headers:
        {{response.RequestMessage.Headers}}
        Content:
        {{response.RequestMessage.Content}}
        }
        """);
}
