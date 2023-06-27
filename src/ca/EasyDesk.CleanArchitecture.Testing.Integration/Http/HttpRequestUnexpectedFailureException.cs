using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public abstract class HttpRequestUnexpectedResultException : Exception
{
    protected HttpRequestUnexpectedResultException(ImmutableHttpResponseMessage response)
        : base(GenerateMessage(response))
    {
    }

    public static string GenerateMessage(ImmutableHttpResponseMessage response) =>
        $$"""
        HttpRequest failed unexpectedly.
        -----<Response>-----
        StatusCode: {{response.StatusCode}}
        Headers:
            {{response.Headers}}
        Content:
            {{response.Content}}
        ---------<>---------

        -----<Request>------
        Method: {{response.RequestMessage.Method}}
        Uri: {{response.RequestMessage.RequestUri}}
        Headers:
            {{response.RequestMessage.Headers}}
        Content:
            {{response.RequestMessage.Content}}
        ---------<>---------
        """;
}

public class HttpRequestUnexpectedFailureException : HttpRequestUnexpectedResultException
{
    public HttpRequestUnexpectedFailureException(ImmutableHttpResponseMessage response) : base(response)
    {
    }
}

public class HttpRequestUnexpectedSuccessException : HttpRequestUnexpectedResultException
{
    public HttpRequestUnexpectedSuccessException(ImmutableHttpResponseMessage response) : base(response)
    {
    }
}
