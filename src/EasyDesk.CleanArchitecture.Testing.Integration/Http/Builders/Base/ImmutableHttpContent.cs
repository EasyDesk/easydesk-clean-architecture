﻿using EasyDesk.Tools.Collections;
using System.Text;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public record class ImmutableHttpContent(
    string Text,
    Option<Encoding> Encoding,
    Option<string> MediaType)
{
    public ImmutableHttpContent(string text)
        : this(text, None, None)
    {
    }

    public ImmutableHttpContent(string text, string mediaType)
        : this(text, None, mediaType.AsOption())
    {
    }

    public ImmutableHttpContent(string text, string mediaType, Encoding encoding)
        : this(text, encoding.AsOption(), mediaType.AsOption())
    {
    }

    private string ToMetadata() => Encoding.Select(e => $"{nameof(Encoding)}: {e}").Concat(MediaType.Select(m => $"{nameof(MediaType)}: {m}")).ConcatStrings(", ", " [", "]");

    public override string ToString() =>
        $"""
        {GetType().Name}{ToMetadata()}:
        {Text}
        """;

    public static async Task<ImmutableHttpContent> From(HttpContent content) =>
        new(await content.AsOption().MapAsync(c => c.ReadAsStringAsync()) | string.Empty, content?.Headers.ContentType.MediaType);
}