using EasyDesk.Commons.Utils;
using System.Collections;
using System.Collections.Immutable;

namespace EasyDesk.Commons.Collections.Immutable;

public abstract class EquatableImmutableSet<T> : IImmutableSet<T>
{
    public EquatableImmutableSet(IImmutableSet<T> set)
    {
        Set = set;
    }

    protected IImmutableSet<T> Set { get; }

    protected abstract IImmutableSet<T> Wrap(IImmutableSet<T> set);

    public IImmutableSet<T> Add(T value) => Wrap(Set.Add(value));

    public IImmutableSet<T> Clear() => Wrap(Set.Clear());

    public bool Contains(T value) => Set.Contains(value);

    public IImmutableSet<T> Except(IEnumerable<T> other) => Wrap(Set.Except(other));

    public IImmutableSet<T> Intersect(IEnumerable<T> other) => Wrap(Set.Intersect(other));

    public bool IsProperSubsetOf(IEnumerable<T> other) => Set.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<T> other) => Set.IsProperSupersetOf(other);

    public bool IsSubsetOf(IEnumerable<T> other) => Set.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<T> other) => Set.IsSupersetOf(other);

    public bool Overlaps(IEnumerable<T> other) => Set.Overlaps(other);

    public IImmutableSet<T> Remove(T value) => Wrap(Set.Remove(value));

    public bool SetEquals(IEnumerable<T> other) => Set.SetEquals(other);

    public IImmutableSet<T> SymmetricExcept(IEnumerable<T> other) => Set.SymmetricExcept(other);

    public bool TryGetValue(T equalValue, out T actualValue) => Set.TryGetValue(equalValue, out actualValue);

    public IImmutableSet<T> Union(IEnumerable<T> other) => Wrap(Set.Union(other));

    public int Count => Set.Count;

    public IEnumerator<T> GetEnumerator() => Set.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Set.GetEnumerator();

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is IImmutableSet<T> other && SetEquals(other));
    }

    public override int GetHashCode() => Set.CombineHashCodes();

    public override string ToString() => Set.ToSetString();
}

public class EquatableImmutableHashSet<T> : EquatableImmutableSet<T>
{
    private readonly IEqualityComparer<T> _comparer;

    private EquatableImmutableHashSet(IImmutableSet<T> set, IEqualityComparer<T> comparer) : base(set)
    {
        _comparer = comparer;
    }

    protected override IImmutableSet<T> Wrap(IImmutableSet<T> set) => new EquatableImmutableHashSet<T>(set, _comparer);

    public override int GetHashCode() => Set.CombineHashCodes(_comparer);

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
