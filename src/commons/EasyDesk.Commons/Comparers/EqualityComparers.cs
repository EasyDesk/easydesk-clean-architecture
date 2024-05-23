using EasyDesk.Commons.Utils;

namespace EasyDesk.Commons.Comparers;

public static class EqualityComparers
{
    public static IEqualityComparer<T> FromComparer<T>(IComparer<T> comparer, Func<T, int>? hashCode = null) =>
        From((x, y) => comparer.Compare(x, y) == 0, hashCode ?? (x => x?.GetHashCode() ?? 0));

    public static IEqualityComparer<T> From<T>(Func<T, T, bool> equals, Func<T, int> hashCode) =>
        new EqualityComparer<T>(equals, hashCode);

    public static IEqualityComparer<T> FromProperties<T>(params Func<T, object>[] properties)
    {
        IEnumerable<object> PropertyValues(T t) => properties.Select(p => p(t));

        bool Equals(T left, T right) => PropertyValues(left).SequenceEqual(PropertyValues(right));

        int HashCode(T t) => PropertyValues(t).CombineHashCodes();

        return From<T>(Equals, HashCode);
    }

    public static IEqualityComparer<KeyValuePair<K, V>> ForKeyValuePair<K, V>(
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
            var keyHash = keyValuePair.Key is null ? 0 : keyComparer.GetHashCode(keyValuePair.Key);
            var valueHash = keyValuePair.Value is null ? 0 : valueComparer.GetHashCode(keyValuePair.Value);
            return keyHash ^ valueHash;
        }

        return From<KeyValuePair<K, V>>(Equals, GetHashCode);
    }

    private class EqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equals;
        private readonly Func<T, int> _hashCode;

        public EqualityComparer(Func<T, T, bool> equals, Func<T, int> hashCode)
        {
            _equals = equals;
            _hashCode = hashCode;
        }

        public bool Equals(T? x, T? y) => ReferenceEquals(x, y) || x is not null && y is not null && _equals(x, y);

        public int GetHashCode(T obj) => _hashCode(obj);
    }
}
