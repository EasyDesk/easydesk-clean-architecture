using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using System.Collections.Immutable;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public record AttributeCollection(IImmutableDictionary<string, IImmutableSet<string>> AttributeMap)
{
    public static AttributeCollection Empty { get; } = new(Map<string, IImmutableSet<string>>());

    public IImmutableSet<string> GetValues(string key) => AttributeMap
        .GetOption(key)
        .OrElseGet(() => Set<string>());

    public Option<string> GetSingle(string key) => AttributeMap
        .GetOption(key)
        .FlatMap(v => v.FirstOption());

    public static AttributeCollection FromFlatKeyValuePairs(IEnumerable<(string Key, string Value)> pairs)
    {
        return new(pairs
            .GroupBy(x => x.Key, x => x.Value)
            .ToEquatableMap(x => x.Key, x => x.ToEquatableSet()));
    }

    public static AttributeCollection FromFlatKeyValuePairs(params (string Key, string Value)[] pairs) =>
        FromFlatKeyValuePairs(pairs.AsEnumerable());
}
