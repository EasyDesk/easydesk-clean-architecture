using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using System.Net.Http.Headers;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public record ImmutableHttpHeaders(
    IFixedMap<string, IFixedList<string>> Map)
{
    public static ImmutableHttpHeaders Empty { get; } = new(Map<string, IFixedList<string>>());

    public static ImmutableHttpHeaders FromHttpHeaders(HttpHeaders headers) =>
        new(headers.ToFixedMap(x => x.Key, x => x.Value.ToFixedList()));

    public ImmutableHttpHeaders Replace(string header, params IFixedList<string> value) =>
        new(Map.SetItem(header, value));

    public ImmutableHttpHeaders Add(string header, string value) =>
        new(Map.Update(header, x => x.Add(value), () => [value]));

    public ImmutableHttpHeaders Remove(string header) =>
        new(Map.Remove(header));

    public override string ToString() => Map.Select(pair => $"{pair.Key}: {pair.Value.ConcatStrings(", ")}").ConcatStrings("\n");
}
