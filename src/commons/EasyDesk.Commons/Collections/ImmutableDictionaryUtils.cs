using EasyDesk.Commons.Options;
using System.Collections.Immutable;
using System.Diagnostics;

namespace EasyDesk.Commons.Collections;

public static class ImmutableDictionaryUtils
{
    public static Option<V> GetOption<K, V>(this IImmutableDictionary<K, V> dictionary, K key)
        where K : notnull =>
        TryOption<K, V>(dictionary.TryGetValue, key);

    public static IImmutableDictionary<K, V> Merge<K, V>(
        this IImmutableDictionary<K, V> dictionary,
        K key,
        V value,
        Func<V, V, V> combiner)
        where K : notnull
    {
        return dictionary.Update(key, v => combiner(v, value), () => value);
    }

    public static IImmutableDictionary<K, V> AddIfAbsent<K, V>(this IImmutableDictionary<K, V> dictionary, K key, Func<V> value) =>
        dictionary.ContainsKey(key) ? dictionary : dictionary.Add(key, value());

    public static IImmutableDictionary<K, V> Update<K, V>(
        this IImmutableDictionary<K, V> dictionary,
        K key,
        Func<V, V> mutation,
        Func<V> supplier)
        where K : notnull
    {
        return dictionary.SetItem(key, dictionary.GetOption(key).Match(
            some: mutation,
            none: supplier));
    }

    public static IImmutableDictionary<K, V> UpdateIfPresent<K, V>(
        this IImmutableDictionary<K, V> dictionary,
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

    public static IImmutableDictionary<K, V> UpdateOption<K, V>(
        this IImmutableDictionary<K, V> dictionary,
        K key,
        Func<Option<V>, Option<V>> mutation)
        where K : notnull
    {
        var old = dictionary.GetOption(key);
        return mutation(old).Match(
            some: v => dictionary.SetItem(key, v),
            none: () => old.Match(
                some: _ => dictionary.Remove(key),
                none: () => dictionary));
    }
}
