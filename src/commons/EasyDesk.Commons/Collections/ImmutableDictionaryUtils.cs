using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using System.Diagnostics;

namespace EasyDesk.Commons.Collections;

public static class ImmutableDictionaryUtils
{
    public static IFixedMap<K, V> Merge<K, V>(
        this IFixedMap<K, V> dictionary,
        K key,
        V value,
        Func<V, V, V> combiner)
        where K : notnull
    {
        return dictionary.Update(key, v => combiner(v, value), () => value);
    }

    public static IFixedMap<K, V> AddIfAbsent<K, V>(this IFixedMap<K, V> dictionary, K key, Func<V> value) =>
        dictionary.ContainsKey(key) ? dictionary : dictionary.Add(key, value());

    public static IFixedMap<K, V> Update<K, V>(
        this IFixedMap<K, V> dictionary,
        K key,
        Func<V, V> mutation,
        Func<V> supplier)
        where K : notnull
    {
        return dictionary.SetItem(key, dictionary.Get(key).Match(
            some: mutation,
            none: supplier));
    }

    public static IFixedMap<K, V> UpdateIfPresent<K, V>(
        this IFixedMap<K, V> dictionary,
        K key,
        Func<V, V> mutation)
        where K : notnull
    {
        if (dictionary.ContainsKey(key))
        {
            return dictionary;
        }
        return dictionary.Update(key, mutation, () => throw new UnreachableException());
    }

    public static IFixedMap<K, V> UpdateOption<K, V>(
        this IFixedMap<K, V> dictionary,
        K key,
        Func<Option<V>, Option<V>> mutation)
        where K : notnull
    {
        var old = dictionary.Get(key);
        return mutation(old).Match(
            some: v => dictionary.SetItem(key, v),
            none: () => old.Match(
                some: _ => dictionary.Remove(key),
                none: () => dictionary));
    }
}
