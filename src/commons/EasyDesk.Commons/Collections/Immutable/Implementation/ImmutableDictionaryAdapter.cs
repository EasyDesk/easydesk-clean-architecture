using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Utils;
using System.Collections;
using System.Collections.Immutable;

namespace EasyDesk.Commons.Collections.Immutable.Implementation;

internal class ImmutableDictionaryAdapter<K, V> : IFixedMap<K, V>
{
    private readonly IImmutableDictionary<K, V> _dictionary;
    private readonly IEqualityComparer<KeyValuePair<K, V>> _keyValuePairComparer;

    public ImmutableDictionaryAdapter(IImmutableDictionary<K, V> dictionary, IEqualityComparer<KeyValuePair<K, V>> keyValuePairComparer)
    {
        _dictionary = dictionary;
        _keyValuePairComparer = keyValuePairComparer;
    }

    private ImmutableDictionaryAdapter<K, V> Wrap(IImmutableDictionary<K, V> dictionary) =>
        new(dictionary, _keyValuePairComparer);

    public Option<V> Get(K key) => TryOption<K, V>(_dictionary.TryGetValue, key);

    public IEnumerable<K> Keys => _dictionary.Keys;

    public IEnumerable<V> Values => _dictionary.Values;

    public IFixedMap<K, V> Add(K key, V value) => Wrap(_dictionary.Add(key, value));

    public IFixedMap<K, V> AddRange(IEnumerable<KeyValuePair<K, V>> pairs) => Wrap(_dictionary.AddRange(pairs));

    public IFixedMap<K, V> Remove(K key) => Wrap(_dictionary.Remove(key));

    public IFixedMap<K, V> RemoveRange(IEnumerable<K> keys) => Wrap(_dictionary.RemoveRange(keys));

    public IFixedMap<K, V> SetItem(K key, V value) => Wrap(_dictionary.SetItem(key, value));

    public IFixedMap<K, V> SetItems(IEnumerable<KeyValuePair<K, V>> items) => Wrap(_dictionary.SetItems(items));

    public IFixedMap<K, V> Clear() => Wrap(_dictionary.Clear());

    public bool ContainsKey(K key) => _dictionary.ContainsKey(key);

    public bool Contains(KeyValuePair<K, V> pair) => _dictionary.Contains(pair);

    public int Count => _dictionary.Count;

    public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => _dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(IFixedMap<K, V>? other) =>
        ReferenceEquals(this, other) || other is not null && EqualsImpl(other);

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || obj is IFixedMap<K, V> other && EqualsImpl(other);

    private bool EqualsImpl(IFixedMap<K, V> other) =>
        Count == other.Count && !this.Except(other, _keyValuePairComparer).Any();

    public override int GetHashCode() => _dictionary.CombineHashCodes(_keyValuePairComparer);

    public override string ToString() => _dictionary.ToSetString(kvp => $"{kvp.Key} -> {kvp.Value}");

    public IImmutableDictionary<K, V> AsImmutableDictionary() => _dictionary;
}
