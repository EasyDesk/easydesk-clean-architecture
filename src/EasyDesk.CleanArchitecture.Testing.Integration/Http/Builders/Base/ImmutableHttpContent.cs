using EasyDesk.Tools.Collections;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Text;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public record class ImmutableHttpContent(
    string Text,
    Option<string> MediaType,
    Option<Encoding> Encoding)
{
    public ImmutableHttpContent(string text)
        : this(text, None, None)
    {
    }

    public ImmutableHttpContent(string text, string mediaType)
        : this(text, mediaType.AsOption(), None)
    {
    }

    public ImmutableHttpContent(string text, string mediaType, Encoding encoding)
        : this(text, mediaType.AsOption(), encoding.AsOption())
    {
    }

    private string ToMetadata() =>
        Encoding
            .Select(e => $"{nameof(Encoding)}: {e}")
            .Concat(
                MediaType
                    .Select(m => $"{nameof(MediaType)}: {m}"))
            .ConcatStrings(", ", " [", "]");

    private string ToText() =>
        MediaType.Any(m => m == MediaTypeNames.Application.Json)
        ? JsonConvert.SerializeObject(JsonConvert.DeserializeObject(Text), Formatting.Indented)
        : Text;

    public override string ToString() =>
        $"""
        {GetType().Name}{ToMetadata()}:
        {ToText()}
        """;

    public static async Task<ImmutableHttpContent> From(HttpContent content) =>
        new(await content.AsOption().MapAsync(c => c.ReadAsStringAsync()) | string.Empty, content?.Headers.ContentType?.MediaType);
}
