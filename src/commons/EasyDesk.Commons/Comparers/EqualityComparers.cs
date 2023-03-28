using EasyDesk.Commons.Utils;

namespace EasyDesk.Commons;

public static class EqualityComparers
{
    public static IEqualityComparer<T> From<T>(Func<T, T, bool> equals, Func<T, int> hashCode) =>
        new EqualityComparer<T>(equals, hashCode);

    public static IEqualityComparer<T> FromProperties<T>(params Func<T, object>[] properties)
    {
        IEnumerable<object> PropertyValues(T t) => properties.Select(p => p(t));

        bool Equals(T left, T right) => Enumerable.SequenceEqual(PropertyValues(left), PropertyValues(right));

        int HashCode(T t) => PropertyValues(t).CombineHashCodes();

        return From<T>(Equals, HashCode);
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

        public bool Equals(T? x, T? y) => ReferenceEquals(x, y) || (x is not null && y is not null && _equals(x, y));

        public int GetHashCode(T obj) => _hashCode(obj);
    }
}
