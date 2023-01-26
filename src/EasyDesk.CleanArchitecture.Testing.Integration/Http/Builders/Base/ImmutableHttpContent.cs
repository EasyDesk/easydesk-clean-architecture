using EasyDesk.Tools.Collections;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Net.Mime;
using System.Text;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public record class ImmutableHttpContent(
    ImmutableArray<byte> Bytes,
    Option<Encoding> TextEncoding,
    Option<string> MediaType)
{
    public ImmutableHttpContent()
        : this(ImmutableArray<byte>.Empty)
    {
    }

    public ImmutableHttpContent(ImmutableArray<byte> bytes)
        : this(bytes, None, None)
    {
    }

    public ImmutableHttpContent(ImmutableArray<byte> bytes, string mediaType)
        : this(bytes, None, mediaType.AsOption())
    {
    }

    public ImmutableHttpContent(ImmutableArray<byte> bytes, Encoding encoding, string mediaType)
        : this(bytes, encoding.AsOption(), mediaType.AsOption())
    {
    }

    public ImmutableHttpContent(string text, string mediaType)
        : this(Encoding.Default.GetBytes(text).ToImmutableArray(), Encoding.Default, mediaType)
    {
    }

    private string ToMetadata() => TextEncoding
        .Select(e => $"{nameof(Encoding)}: {e}")
        .Concat(
            MediaType
                .Select(m => $"{nameof(MediaType)}: {m}"))
        .ConcatStrings(", ", " [", "]");

    public string AsString() => Bytes.IsNullOrEmpty()
        ? string.Empty
        : TextEncoding
            .Map(e => e.GetString(Bytes.ToArray()))
            .OrElseThrow(() => new InvalidOperationException("Can't convert Http content to text because an encoding is missing."));

    private string ToText() =>
        MediaType.Any(m => m == MediaTypeNames.Application.Json)
        ? JsonConvert.SerializeObject(JsonConvert.DeserializeObject(AsString()), Formatting.Indented)
        : AsString();

    public override string ToString() =>
        $"""
        {GetType().Name}{ToMetadata()}:
        {ToText()}
        """;

    public HttpContent ToHttpContent() =>
        TextEncoding.Match(
            some: e => MediaType.Match(
                some: m => new StringContent(e.GetString(Bytes.ToArray()), e, m),
                none: () => new StringContent(e.GetString(Bytes.ToArray()), e)),
            none: () => new ByteArrayContent(Bytes.ToArray()));

    public static async Task<ImmutableHttpContent> From(HttpContent? content) =>
        new(
            Bytes: await content.AsOption().MapAsync(c => c.ReadAsByteArrayAsync()).ThenMap(b => b.ToImmutableArray()) | ImmutableArray<byte>.Empty,
            TextEncoding: content.AsOption().Map(c => c.Headers).FlatMap(h => (h.ContentType?.CharSet).AsOption() || h.ContentEncoding.FirstOption()).Map(e => Encoding.GetEncoding(e)),
            MediaType: content.AsOption().Map(c => c.Headers).FlatMap(h => (h.ContentType?.MediaType).AsOption()));
}
