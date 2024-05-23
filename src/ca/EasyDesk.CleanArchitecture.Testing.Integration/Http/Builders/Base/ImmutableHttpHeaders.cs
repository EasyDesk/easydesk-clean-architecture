using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public record class ImmutableHttpHeaders(
    IFixedMap<string, IEnumerable<string>> Dictionary)
{
    public ImmutableHttpHeaders()
        : this(Map<string, IEnumerable<string>>())
    {
    }

    public ImmutableHttpHeaders Replace(string header, string value) =>
        new(Dictionary
            .SetItem(
                header,
                EnumerableUtils.Items(value)));

    public ImmutableHttpHeaders Add(string header, string value) =>
        new(Dictionary
            .SetItem(
                header,
                Dictionary
                    .Get(header)
                    .OrElseGet(Enumerable.Empty<string>)
                    .Append(value)));

    public ImmutableHttpHeaders Remove(string header) =>
        new(Dictionary.Remove(header));

    public override string ToString() => Dictionary.Select(pair => $"{pair.Key}: {pair.Value.ConcatStrings(", ")}").ConcatStrings("\n");
}
