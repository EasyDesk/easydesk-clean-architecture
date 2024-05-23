using EasyDesk.Commons.Options;
using System.Collections.Immutable;

namespace EasyDesk.Commons.Collections.Immutable;

public interface IFixedMap<K, V> : IReadOnlyCollection<KeyValuePair<K, V>>, IEquatable<IFixedMap<K, V>>
{
    Option<V> Get(K key);

    IEnumerable<K> Keys { get; }

    IEnumerable<V> Values { get; }

    IFixedMap<K, V> Add(K key, V value);

    IFixedMap<K, V> AddRange(IEnumerable<KeyValuePair<K, V>> pairs);

    IFixedMap<K, V> Remove(K key);

    IFixedMap<K, V> RemoveRange(IEnumerable<K> keys);

    IFixedMap<K, V> SetItem(K key, V value);

    IFixedMap<K, V> SetItems(IEnumerable<KeyValuePair<K, V>> items);

    IFixedMap<K, V> Clear();

    bool ContainsKey(K key);

    bool Contains(KeyValuePair<K, V> pair);

    IImmutableDictionary<K, V> AsImmutableDictionary();
}
