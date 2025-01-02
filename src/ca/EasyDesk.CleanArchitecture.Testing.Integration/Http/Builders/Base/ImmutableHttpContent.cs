using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using HeaderNames = Microsoft.Net.Http.Headers.HeaderNames;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public record class ImmutableHttpContent(
    ImmutableArray<byte> Bytes,
    ImmutableHttpHeaders ContentHeaders)
{
    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public ImmutableHttpContent()
        : this([])
    {
    }

    public ImmutableHttpContent(ImmutableArray<byte> bytes)
        : this(bytes, new ImmutableHttpHeaders())
    {
    }

    public ImmutableHttpContent(ImmutableArray<byte> bytes, string mediaType)
        : this(bytes, new ImmutableHttpHeaders().Add(HeaderNames.ContentType, mediaType))
    {
    }

    public ImmutableHttpContent(ImmutableArray<byte> bytes, Encoding encoding, string mediaType)
        : this(bytes, new ImmutableHttpHeaders()
              .Add(HeaderNames.ContentType, mediaType)
              .Add(HeaderNames.ContentEncoding, encoding.WebName))
    {
    }

    public ImmutableHttpContent(string text, string mediaType)
        : this([.. Encoding.Default.GetBytes(text)], Encoding.Default, mediaType)
    {
    }

    public Option<MediaTypeHeaderValue> MediaType => ContentHeaders.Dictionary
        .Get(HeaderNames.ContentType)
        .FlatMap(e => e.FirstOption())
        .Map(MediaTypeHeaderValue.Parse);

    public Option<Encoding> TextEncoding => ContentHeaders.Dictionary
        .Get(HeaderNames.ContentEncoding)
        .FlatMap(e => e.FirstOption())
        .Or(MediaType
            .FlatMap(m => m.CharSet.AsOption()))
        .Map(Encoding.GetEncoding);

    private string ToMetadata() => TextEncoding
        .Select(e => $"{nameof(Encoding)}: {e}")
        .Concat(
            MediaType
                .Select(m => $"{nameof(MediaType)}: {m}"))
        .ConcatStrings(", ", " [", "]");

    public string AsString() => Bytes.IsDefaultOrEmpty
        ? string.Empty
        : TextEncoding
            .Map(e => e.GetString([.. Bytes]))
            .OrElseGet(() => $"BINARY {nameof(ImmutableHttpContent)} in base64:\n{Convert.ToBase64String(Bytes.ToArray())}");

    private string ToText()
    {
        var asString = AsString();
        if (MediaType.Any(m => m.MediaType == MediaTypeNames.Application.Json))
        {
            try
            {
                using var jsonDocument = JsonDocument.Parse(asString);
                return JsonSerializer.Serialize(jsonDocument, _options);
            }
            catch (JsonException)
            {
                return asString;
            }
        }
        return asString;
    }

    public override string ToString() =>
        $"""
        {GetType().Name}{ToMetadata()}:
        {ToText()}
        """;

    public HttpContent ToHttpContent()
    {
        var content = TextEncoding.Match(
            some: e => MediaType.Match(
                some: m => new StringContent(e.GetString([.. Bytes]), e, m),
                none: () => new StringContent(e.GetString([.. Bytes]), e)),
            none: () => new ByteArrayContent([.. Bytes]));
        foreach (var (key, value) in ContentHeaders.Dictionary)
        {
            if (!content.Headers.Contains(key))
            {
                content.Headers.Add(key, value);
            }
        }
        return content;
    }

    public static async Task<ImmutableHttpContent> From(HttpContent? content) =>
        new(
            Bytes: await content
                .AsOption()
                .MapAsync(c => c.ReadAsByteArrayAsync())
                .ThenMap(b => b.ToImmutableArray())
                | ImmutableArray<byte>.Empty,
            ContentHeaders: content
                .AsOption()
                .Map(c => c.Headers.ToFixedMap())
                .Map(d => new ImmutableHttpHeaders(d))
                .OrElseGet(() => new()));
}
