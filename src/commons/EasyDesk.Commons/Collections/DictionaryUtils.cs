namespace EasyDesk.Commons.Collections;

public static class DictionaryUtils
{
    public static Option<V> GetOption<K, V>(this IDictionary<K, V> dictionary, K key)
        where K : notnull
        where V : notnull =>
        TryOption<K, V>(dictionary.TryGetValue, key);

    public static bool Merge<K, V>(this IDictionary<K, V> dictionary, K key, V value, Func<V, V, V> combiner)
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary[key] = combiner(dictionary[key], value);
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
}
