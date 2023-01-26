using System.Collections.Immutable;
using static EasyDesk.Tools.Collections.ImmutableCollections;

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
        : this(method, requestUri, new(Map<string, IEnumerable<string>>()), new())
    {
    }

    public static async Task<ImmutableHttpRequestMessage> From(HttpRequestMessage request) => new(
        request.Method,
        request.RequestUri ?? throw new InvalidOperationException("Request URI is missing."),
        new(request.Headers.ToImmutableDictionary()),
        await ImmutableHttpContent.From(request.Content));

    public HttpRequestMessage ToHttpRequestMessage()
    {
        var request = new HttpRequestMessage(Method, RequestUri)
        {
            Content = Content?.ToHttpContent()
        };
        foreach (var (headerKey, headerValue) in Headers.Dictionary)
        {
            request.Headers.Add(headerKey, headerValue);
        }
        return request;
    }
}
