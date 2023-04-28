using EasyDesk.Commons.Utils;
using System.Collections;
using System.Collections.Immutable;

namespace EasyDesk.Commons.Collections.Immutable;

public abstract class EquatableImmutableSet<T> : IImmutableSet<T>
{
    private readonly IImmutableSet<T> _set;

    public EquatableImmutableSet(IImmutableSet<T> set)
    {
        _set = set;
    }

    protected abstract IImmutableSet<T> Wrap(IImmutableSet<T> set);

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
        return ReferenceEquals(this, obj) || (obj is IImmutableSet<T> other && SetEquals(other));
    }

    public override int GetHashCode() => _set.CombineHashCodes();

    public override string ToString() => _set.ToSetString();
}

public class EquatableImmutableHashSet<T> : EquatableImmutableSet<T>
{
    private readonly IEqualityComparer<T> _comparer;

    private EquatableImmutableHashSet(IImmutableSet<T> set, IEqualityComparer<T> comparer) : base(set)
    {
        _comparer = comparer;
    }

    protected override IImmutableSet<T> Wrap(IImmutableSet<T> set) => new EquatableImmutableHashSet<T>(set, _comparer);

    public override int GetHashCode() => this.CombineHashCodes(_comparer);

    public static EquatableImmutableHashSet<T> Create(ImmutableHashSet<T> set) => new(set, set.KeyComparer);
}

public class EquatableImmutableSortedSet<T> : EquatableImmutableSet<T>
{
    private readonly IComparer<T> _comparer;

    private EquatableImmutableSortedSet(IImmutableSet<T> set, IComparer<T> comparer) : base(set)
    {
        _comparer = comparer;
    }

    protected override IImmutableSet<T> Wrap(IImmutableSet<T> set) => new EquatableImmutableSortedSet<T>(set, _comparer);

    public static EquatableImmutableSortedSet<T> Create(ImmutableSortedSet<T> set) => new(set, set.KeyComparer);
}
