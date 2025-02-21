using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.Authentication;

public record AttributeCollection(IFixedMap<string, IFixedSet<string>> AttributeMap)
{
    public static AttributeCollection Empty { get; } = new(Map<string, IFixedSet<string>>());

    public IFixedSet<string> GetValues(string key) => AttributeMap
        .Get(key)
        .OrElseGet(() => Set<string>());

    public Option<string> GetSingle(string key) => AttributeMap
        .Get(key)
        .FlatMap(v => v.FirstOption());

    public static AttributeCollection FromFlatKeyValuePairs(IEnumerable<(string Key, string Value)> pairs)
    {
        return new(pairs
            .GroupBy(x => x.Key, x => x.Value)
            .ToFixedMap(x => x.Key, x => x.ToFixedSet()));
    }

    public static AttributeCollection FromFlatKeyValuePairs(params (string Key, string Value)[] pairs) =>
        FromFlatKeyValuePairs(pairs.AsEnumerable());
}
