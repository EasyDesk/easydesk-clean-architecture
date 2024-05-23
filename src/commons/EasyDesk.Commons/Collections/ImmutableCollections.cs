using EasyDesk.Commons.Collections.Immutable;
using System.Collections.Immutable;

namespace EasyDesk.Commons.Collections;

public static class ImmutableCollections
{
    public static IFixedSet<T> ToFixedSet<T>(this IEnumerable<T> items) => Set(items);

    public static IFixedSet<T> ToFixedHashSet<T>(this IEnumerable<T> items, IEqualityComparer<T>? comparer = null) => HashSet(items, comparer);

    public static IFixedSet<T> ToFixedSortedSet<T>(this IEnumerable<T> items, IComparer<T>? comparer = null) => SortedSet(items, comparer);

    public static IFixedSet<T> Set<T>(params T[] items) => Set(items.AsEnumerable());

    public static IFixedSet<T> Set<T>(IEnumerable<T> items) => HashSet(items);

    public static IFixedSet<T> HashSet<T>(IEnumerable<T> items, IEqualityComparer<T>? comparer = null)
    {
        var set = ImmutableHashSet.CreateRange(comparer, items);
        return FixedHashSet.Create(set);
    }

    public static IFixedSet<T> SortedSet<T>(IEnumerable<T> items, IComparer<T>? comparer = null)
    {
        var set = ImmutableSortedSet.CreateRange(comparer, items);
        return FixedSortedSet.Create(set);
    }

    public static IFixedList<T> ToFixedList<T>(this IEnumerable<T> items) => List(items);

    public static IFixedList<T> List<T>(params T[] items) => List(items.AsEnumerable());

    public static IFixedList<T> List<T>(IEnumerable<T> items)
    {
        var list = ImmutableList.CreateRange(items);
        return FixedList.Create(list);
    }

    public static IFixedMap<K, V> ToFixedMap<K, V>(this IEnumerable<(K, V)> items) where K : notnull =>
        items.ToFixedHashMap();

    public static IFixedMap<K, V> ToFixedMap<K, V>(this IEnumerable<KeyValuePair<K, V>> items) where K : notnull =>
        items.ToFixedHashMap();

    public static IFixedMap<K, V> ToFixedMap<K, V>(
        this IEnumerable<V> items,
        Func<V, K> key) where K : notnull =>
        items.ToFixedHashMap(key);

    public static IFixedMap<K, V> ToFixedMap<T, K, V>(
        this IEnumerable<T> items,
        Func<T, K> key,
        Func<T, V> value) where K : notnull =>
        items.ToFixedHashMap(key, value);

    public static IFixedMap<K, V> ToFixedHashMap<K, V>(
        this IEnumerable<(K, V)> items,
        IEqualityComparer<K>? keyComparer = null,
        IEqualityComparer<V>? valueComparer = null) where K : notnull =>
        HashMap(items, keyComparer, valueComparer);

    public static IFixedMap<K, V> ToFixedHashMap<K, V>(
        this IEnumerable<KeyValuePair<K, V>> items,
        IEqualityComparer<K>? keyComparer = null,
        IEqualityComparer<V>? valueComparer = null) where K : notnull =>
        HashMap(items, keyComparer, valueComparer);

    public static IFixedMap<K, V> ToFixedHashMap<K, V>(
        this IEnumerable<V> items,
        Func<V, K> key,
        IEqualityComparer<K>? keyComparer = null,
        IEqualityComparer<V>? valueComparer = null) where K : notnull =>
        items.ToFixedHashMap(key, It, keyComparer, valueComparer);

    public static IFixedMap<K, V> ToFixedHashMap<T, K, V>(
        this IEnumerable<T> items,
        Func<T, K> key,
        Func<T, V> value,
        IEqualityComparer<K>? keyComparer = null,
        IEqualityComparer<V>? valueComparer = null) where K : notnull =>
        FixedHashMap.Create(items.ToImmutableDictionary(key, value, keyComparer, valueComparer));

    public static IFixedMap<K, V> ToFixedSortedMap<K, V>(
        this IEnumerable<(K, V)> items,
        IComparer<K>? keyComparer = null,
        IEqualityComparer<V>? valueComparer = null) where K : notnull =>
        SortedMap(items, keyComparer, valueComparer);

    public static IFixedMap<K, V> ToFixedSortedMap<K, V>(
        this IEnumerable<KeyValuePair<K, V>> items,
        IComparer<K>? keyComparer = null,
        IEqualityComparer<V>? valueComparer = null) where K : notnull =>
        SortedMap(items, keyComparer, valueComparer);

    public static IFixedMap<K, V> ToFixedSortedMap<K, V>(
        this IEnumerable<V> items,
        Func<V, K> key,
        IComparer<K>? keyComparer = null,
        IEqualityComparer<V>? valueComparer = null) where K : notnull =>
        items.ToFixedSortedMap(key, It, keyComparer, valueComparer);

    public static IFixedMap<K, V> ToFixedSortedMap<T, K, V>(
        this IEnumerable<T> items,
        Func<T, K> key,
        Func<T, V> value,
        IComparer<K>? keyComparer = null,
        IEqualityComparer<V>? valueComparer = null) where K : notnull =>
        FixedSortedMap.Create(items.ToImmutableSortedDictionary(key, value, keyComparer, valueComparer));

    public static IFixedMap<K, V> Map<K, V>(params (K, V)[] items) where K : notnull =>
        HashMap(items);

    public static IFixedMap<K, V> Map<K, V>(IEnumerable<(K, V)> items) where K : notnull =>
        HashMap(items);

    public static IFixedMap<K, V> Map<K, V>(IEnumerable<KeyValuePair<K, V>> pairs) where K : notnull =>
        HashMap(pairs);

    public static IFixedMap<K, V> HashMap<K, V>(params (K, V)[] items) where K : notnull =>
        HashMap(items.AsEnumerable());

    public static IFixedMap<K, V> HashMap<K, V>(
        IEnumerable<KeyValuePair<K, V>> items,
        IEqualityComparer<K>? keyComparer = null,
        IEqualityComparer<V>? valueComparer = null)
        where K : notnull
    {
        var dictionary = ImmutableDictionary.CreateRange(keyComparer, valueComparer, items);
        return FixedHashMap.Create(dictionary);
    }

    public static IFixedMap<K, V> HashMap<K, V>(
        IEnumerable<(K, V)> items,
        IEqualityComparer<K>? keyComparer = null,
        IEqualityComparer<V>? valueComparer = null)
        where K : notnull =>
        HashMap(ConvertToKeyValuePairs(items), keyComparer, valueComparer);

    public static IFixedMap<K, V> SortedMap<K, V>(params (K, V)[] items) where K : notnull =>
        SortedMap(items.AsEnumerable());

    public static IFixedMap<K, V> SortedMap<K, V>(
        IEnumerable<(K, V)> items,
        IComparer<K>? keyComparer = null,
        IEqualityComparer<V>? valueComparer = null) where K : notnull =>
        SortedMap(ConvertToKeyValuePairs(items), keyComparer, valueComparer);

    public static IFixedMap<K, V> SortedMap<K, V>(
        IEnumerable<KeyValuePair<K, V>> items,
        IComparer<K>? keyComparer = null,
        IEqualityComparer<V>? valueComparer = null) where K : notnull
    {
        var dictionary = ImmutableSortedDictionary.CreateRange(keyComparer, valueComparer, items);
        return FixedSortedMap.Create(dictionary);
    }

    private static IEnumerable<KeyValuePair<K, V>> ConvertToKeyValuePairs<K, V>(IEnumerable<(K Key, V Value)> items) =>
        items.Select(x => new KeyValuePair<K, V>(x.Key, x.Value));
}
