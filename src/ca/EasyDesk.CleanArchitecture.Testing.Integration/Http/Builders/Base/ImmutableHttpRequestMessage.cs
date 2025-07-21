namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public record ImmutableHttpRequestMessage(
    HttpMethod Method,
    Uri RequestUri,
    ImmutableHttpHeaders Headers,
    ImmutableHttpContent? Content)
{
    public ImmutableHttpRequestMessage(HttpMethod method, string requestUri)
        : this(method, new Uri(requestUri, UriKind.RelativeOrAbsolute))
    {
    }

    public ImmutableHttpRequestMessage(HttpMethod method, Uri requestUri)
        : this(method, requestUri, ImmutableHttpHeaders.Empty, new())
    {
    }

    public static async Task<ImmutableHttpRequestMessage> From(HttpRequestMessage request) => new(
        request.Method,
        request.RequestUri ?? throw new InvalidOperationException("Request URI is missing."),
        ImmutableHttpHeaders.FromHttpHeaders(request.Headers),
        await ImmutableHttpContent.From(request.Content));

    public HttpRequestMessage ToHttpRequestMessage()
    {
        var request = new HttpRequestMessage(Method, RequestUri)
        {
            Content = Content?.ToHttpContent(),
        };
        foreach (var (headerKey, headerValue) in Headers.Map)
        {
            request.Headers.Add(headerKey, headerValue);
        }
        return request;
    }
}
