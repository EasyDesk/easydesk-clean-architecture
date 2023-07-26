using System.Collections.Immutable;

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
}
