using EasyDesk.Commons.Collections.Immutable;
using System.Collections.Immutable;

namespace EasyDesk.Commons.Collections;

public static class ImmutableCollections
{
    public delegate IImmutableSet<T> SetMutation<T>(IImmutableSet<T> set);

    public delegate IImmutableList<T> ListMutation<T>(IImmutableList<T> list);

    public delegate IImmutableDictionary<K, V> DictionaryMutation<K, V>(IImmutableDictionary<K, V> dictionary);

    public static IImmutableSet<T> ToEquatableSet<T>(this IEnumerable<T> items) => Set(items);

    public static IImmutableSet<T> ToEquatableSet<T>(this IEnumerable<T> items, IEqualityComparer<T> comparer) => Set(items, comparer);

    public static IImmutableSet<T> Set<T>(params T[] items) => Set(items as IEnumerable<T>);

    public static IImmutableSet<T> Set<T>(IEnumerable<T> items) => Set(items, EqualityComparer<T>.Default);

    public static IImmutableSet<T> Set<T>(IEnumerable<T> items, IEqualityComparer<T> comparer)
    {
        var set = ImmutableHashSet.CreateRange(comparer, items);
        return EquatableImmutableSet<T>.FromHashSet(set);
    }

    public static IImmutableList<T> ToEquatableList<T>(this IEnumerable<T> items) => List(items);

    public static IImmutableList<T> List<T>(params T[] items) => List(items as IEnumerable<T>);

    public static IImmutableList<T> List<T>(IEnumerable<T> items)
    {
        var list = ImmutableList.CreateRange(items);
        return EquatableImmutableList<T>.FromList(list);
    }

    public static IImmutableDictionary<K, V> ToEquatableMap<K, V>(this IEnumerable<(K Key, V Value)> items)
        where K : notnull => Map(items);

    public static IImmutableDictionary<K, V> ToEquatableMap<K, V>(this IEnumerable<KeyValuePair<K, V>> items)
        where K : notnull => Map(items);

    public static IImmutableDictionary<K, V> Map<K, V>(params (K Key, V Value)[] items)
        where K : notnull =>
        Map(items as IEnumerable<(K, V)>);

    public static IImmutableDictionary<K, V> Map<K, V>(IEnumerable<(K Key, V Value)> items)
        where K : notnull
    {
        var pairs = items.Select(x => new KeyValuePair<K, V>(x.Key, x.Value));
        return Map(pairs);
    }

    public static IImmutableDictionary<K, V> Map<K, V>(IEnumerable<KeyValuePair<K, V>> pairs)
        where K : notnull
    {
        var dictionary = ImmutableDictionary.CreateRange(pairs);
        return EquatableImmutableDictionary<K, V>.FromDictionary(dictionary);
    }
}
