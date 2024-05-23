using EasyDesk.Commons.Collections;
using System.Net;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public record ImmutableHttpResponseMessage(
    HttpStatusCode StatusCode,
    ImmutableHttpHeaders Headers,
    ImmutableHttpContent Content,
    ImmutableHttpRequestMessage RequestMessage)
{
    public bool IsSuccessStatusCode => (int)StatusCode is >= 200 and <= 299;

    public static async Task<ImmutableHttpResponseMessage> From(HttpResponseMessage response) => new(
        response.StatusCode,
        new(response.Headers.ToFixedMap()),
        await ImmutableHttpContent.From(response.Content),
        await ImmutableHttpRequestMessage.From(response.RequestMessage ?? throw new InvalidOperationException("Request message is missing.")));
}

public static class HttpResponseMessageExtensions
{
    public static async Task<ImmutableHttpResponseMessage> DisposeAndConvert(this HttpResponseMessage response)
    {
        var r = await ImmutableHttpResponseMessage.From(response);
        response.Dispose();
        return r;
    }
}
