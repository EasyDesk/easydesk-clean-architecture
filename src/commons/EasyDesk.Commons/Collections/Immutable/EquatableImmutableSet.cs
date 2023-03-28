using EasyDesk.Commons.Utils;
using System.Collections;
using System.Collections.Immutable;

namespace EasyDesk.Commons.Collections.Immutable;

public class EquatableImmutableSet<T> : IImmutableSet<T>
{
    private readonly IImmutableSet<T> _set;
    private readonly IEqualityComparer<T> _comparer;

    private EquatableImmutableSet(IImmutableSet<T> set, IEqualityComparer<T> comparer)
    {
        _set = set;
        _comparer = comparer;
    }

    public static EquatableImmutableSet<T> FromHashSet(ImmutableHashSet<T> hashSet) =>
        new(hashSet, hashSet.KeyComparer);

    private EquatableImmutableSet<T> Wrap(IImmutableSet<T> set) => new(set, _comparer);

    public IImmutableSet<T> Add(T value) => Wrap(_set.Add(value));

    public IImmutableSet<T> Clear() => Wrap(_set.Clear());

    public bool Contains(T value) => _set.Contains(value);

    public IImmutableSet<T> Except(IEnumerable<T> other) => Wrap(_set.Except(other));

    public IImmutableSet<T> Intersect(IEnumerable<T> other) => Wrap(_set.Intersect(other));

    public bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);

    public bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);

    public bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);

    public IImmutableSet<T> Remove(T value) => Wrap(_set.Remove(value));

    public bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);

    public IImmutableSet<T> SymmetricExcept(IEnumerable<T> other) => _set.SymmetricExcept(other);

    public bool TryGetValue(T equalValue, out T actualValue) => _set.TryGetValue(equalValue, out actualValue);

    public IImmutableSet<T> Union(IEnumerable<T> other) => Wrap(_set.Union(other));

    public int Count => _set.Count;

    public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _set.GetEnumerator();

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (
            obj is IImmutableSet<T> other
            && SetEquals(other));
    }

    public override int GetHashCode() => _set.CombineHashCodes(_comparer);

    public override string ToString() => _set.ToSetString();
}
