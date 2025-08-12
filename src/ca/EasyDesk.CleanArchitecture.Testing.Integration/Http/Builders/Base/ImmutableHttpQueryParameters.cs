using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public record ImmutableHttpQueryParameters(
    IFixedMap<string, IFixedList<string>> Map)
{
    public static ImmutableHttpQueryParameters Empty { get; } = new(Map<string, IFixedList<string>>());

    public ImmutableHttpQueryParameters Replace(string param, params IFixedList<string> value) =>
        new(Map.SetItem(param, value));

    public ImmutableHttpQueryParameters Add(string header, string value) =>
        new(Map.Update(header, x => x.Add(value), () => [value,]));

    public ImmutableHttpQueryParameters Remove(string header) =>
        new(Map.Remove(header));
}
