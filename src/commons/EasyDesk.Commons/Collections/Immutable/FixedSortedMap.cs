using EasyDesk.Commons.Collections.Immutable.Implementation;
using EasyDesk.Commons.Comparers;
using System.Collections.Immutable;

namespace EasyDesk.Commons.Collections.Immutable;

public static class FixedSortedMap
{
    public static IFixedMap<K, V> Create<K, V>(ImmutableSortedDictionary<K, V> dictionary) where K : notnull =>
        new ImmutableDictionaryAdapter<K, V>(
            dictionary,
            EqualityComparers.ForKeyValuePair(EqualityComparers.FromComparer(dictionary.KeyComparer), dictionary.ValueComparer));
}
