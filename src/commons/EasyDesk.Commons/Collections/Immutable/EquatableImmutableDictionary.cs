using EasyDesk.Commons.Comparers;
using EasyDesk.Commons.Utils;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace EasyDesk.Commons.Collections.Immutable;

public class EquatableImmutableDictionary<K, V> : IImmutableDictionary<K, V>
    where K : notnull
{
    private readonly IImmutableDictionary<K, V> _dictionary;
    private readonly IEqualityComparer<KeyValuePair<K, V>> _keyValuePairComparer;

    private EquatableImmutableDictionary(
        IImmutableDictionary<K, V> dictionary,
        IEqualityComparer<K> keyComparer,
        IEqualityComparer<V> valueComparer)
    {
        _dictionary = dictionary;
        _keyValuePairComparer = CreateKeyValuePairComparer(keyComparer, valueComparer);
    }

    private EquatableImmutableDictionary(IImmutableDictionary<K, V> dictionary, IEqualityComparer<KeyValuePair<K, V>> keyValuePairComparer)
    {
        _dictionary = dictionary;
        _keyValuePairComparer = keyValuePairComparer;
    }

    public static EquatableImmutableDictionary<K, V> FromDictionary(ImmutableDictionary<K, V> dictionary) =>
        new(dictionary, dictionary.KeyComparer, dictionary.ValueComparer);

    private IEqualityComparer<KeyValuePair<K, V>> CreateKeyValuePairComparer(
        IEqualityComparer<K> keyComparer,
        IEqualityComparer<V> valueComparer)
    {
        bool Equals(KeyValuePair<K, V> left, KeyValuePair<K, V> right)
        {
            return keyComparer.Equals(left.Key, right.Key)
                && valueComparer.Equals(left.Value, right.Value);
        }
        int GetHashCode(KeyValuePair<K, V> keyValuePair)
        {
            var keyHash = keyComparer.GetHashCode(keyValuePair.Key);
            return keyValuePair.Value is not null ? keyHash ^ valueComparer.GetHashCode(keyValuePair.Value) : keyHash;
        }

        return EqualityComparers.From<KeyValuePair<K, V>>(Equals, GetHashCode);
    }

    private EquatableImmutableDictionary<K, V> Wrap(IImmutableDictionary<K, V> rawDictionary) =>
        new(rawDictionary, _keyValuePairComparer);

    public IImmutableDictionary<K, V> Add(K key, V value) => Wrap(_dictionary.Add(key, value));

    public IImmutableDictionary<K, V> AddRange(IEnumerable<KeyValuePair<K, V>> pairs) => Wrap(_dictionary.AddRange(pairs));

    public IImmutableDictionary<K, V> Clear() => Wrap(_dictionary.Clear());

    public bool Contains(KeyValuePair<K, V> pair) => _dictionary.Contains(pair);

    public IImmutableDictionary<K, V> Remove(K key) => Wrap(_dictionary.Remove(key));

    public IImmutableDictionary<K, V> RemoveRange(IEnumerable<K> keys) => Wrap(_dictionary.RemoveRange(keys));

    public IImmutableDictionary<K, V> SetItem(K key, V value) => Wrap(_dictionary.SetItem(key, value));

    public IImmutableDictionary<K, V> SetItems(IEnumerable<KeyValuePair<K, V>> items) => Wrap(_dictionary.SetItems(items));

    public bool TryGetKey(K equalKey, out K actualKey) => _dictionary.TryGetKey(equalKey, out actualKey);

    public bool ContainsKey(K key) => _dictionary.ContainsKey(key);

    public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value) => _dictionary.TryGetValue(key, out value);

    public V this[K key] => _dictionary[key];

    public IEnumerable<K> Keys => _dictionary.Keys;

    public IEnumerable<V> Values => _dictionary.Values;

    public int Count => _dictionary.Count;

    public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => _dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (
            obj is IImmutableDictionary<K, V> other
            && Count == other.Count
            && !this.Except(other, _keyValuePairComparer).Any());
    }

    public override int GetHashCode() => _dictionary.CombineHashCodes(_keyValuePairComparer);

    public override string ToString() => _dictionary.ToSetString(kvp => $"{kvp.Key} -> {kvp.Value}");
}
