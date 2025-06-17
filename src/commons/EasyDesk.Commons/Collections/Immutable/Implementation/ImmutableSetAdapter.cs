using EasyDesk.Commons.Utils;
using System.Collections;
using System.Collections.Immutable;

namespace EasyDesk.Commons.Collections.Immutable.Implementation;

internal class ImmutableSetAdapter<T> : IFixedSet<T>
{
    private readonly IImmutableSet<T> _set;
    private readonly IEqualityComparer<T> _equalityComparer;

    public ImmutableSetAdapter(IImmutableSet<T> set, IEqualityComparer<T> equalityComparer)
    {
        _set = set;
        _equalityComparer = equalityComparer;
    }

    private ImmutableSetAdapter<T> Wrap(IImmutableSet<T> set) => new(set, _equalityComparer);

    public IFixedSet<T> Add(T item) => Wrap(_set.Add(item));

    public IFixedSet<T> Clear() => Wrap(_set.Clear());

    public bool Contains(T item) => _set.Contains(item);

    public IFixedSet<T> Except(IEnumerable<T> other) => Wrap(_set.Except(other));

    public IFixedSet<T> Intersect(IEnumerable<T> other) => Wrap(_set.Intersect(other));

    public bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);

    public bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);

    public bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);

    public IFixedSet<T> Remove(T item) => Wrap(_set.Remove(item));

    public bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);

    public IFixedSet<T> SymmetricExcept(IEnumerable<T> other) => Wrap(_set.SymmetricExcept(other));

    public bool TryGetValue(T equalValue, out T actualValue) => _set.TryGetValue(equalValue, out actualValue);

    public IFixedSet<T> Union(IEnumerable<T> other) => Wrap(_set.Union(other));

    public int Count => _set.Count;

    public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _set.GetEnumerator();

    public bool Equals(IFixedSet<T>? other) =>
        ReferenceEquals(this, other) || other is not null && EqualsImpl(other);

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || obj is IFixedSet<T> other && EqualsImpl(other);

    private bool EqualsImpl(IFixedSet<T> other) => SetEquals(other);

    public override int GetHashCode() => _set.CombineHashCodes(_equalityComparer);

    public override string ToString() => _set.ToSetString();

    public IImmutableSet<T> AsImmutableSet() => _set;
}
