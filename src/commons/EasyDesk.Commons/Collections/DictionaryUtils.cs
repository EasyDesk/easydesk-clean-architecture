using EasyDesk.Commons.Options;

namespace EasyDesk.Commons.Collections;

public static class DictionaryUtils
{
    public static Option<V> GetOption<K, V>(this IDictionary<K, V> dictionary, K key) =>
        TryOption<K, V>(dictionary.TryGetValue, key);

    public static bool Merge<K, V>(this IDictionary<K, V> dictionary, K key, V value, Func<V, V, V> combiner)
    {
        if (dictionary.TryGetValue(key, out var existingValue))
        {
            dictionary[key] = combiner(existingValue, value);
            return false;
        }
        else
        {
            dictionary.Add(key, value);
            return true;
        }
    }

    public static V GetOrAdd<K, V>(this IDictionary<K, V> dictionary, K key, Func<V> supplier)
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }
        else
        {
            var newValue = supplier();
            dictionary.Add(key, newValue);
            return newValue;
        }
    }

    public static void Update<K, V>(
        this IDictionary<K, V> dictionary,
        K key,
        Func<V, V> mutation,
        Func<V> supplier)
    {
        dictionary[key] = dictionary.GetOption(key).Match(
            some: mutation,
            none: supplier);
    }
}
