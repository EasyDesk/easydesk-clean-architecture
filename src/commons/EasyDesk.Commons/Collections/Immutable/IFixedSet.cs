using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace EasyDesk.Commons.Collections.Immutable;

[CollectionBuilder(typeof(FixedHashSet), nameof(FixedHashSet.FromSpan))]
public interface IFixedSet<T> : IReadOnlyCollection<T>, IEquatable<IFixedSet<T>>
{
    IFixedSet<T> Add(T item);

    IFixedSet<T> Remove(T item);

    IFixedSet<T> Clear();

    bool Contains(T item);

    IFixedSet<T> Intersect(IEnumerable<T> other);

    IFixedSet<T> Union(IEnumerable<T> other);

    IFixedSet<T> Except(IEnumerable<T> other);

    IFixedSet<T> SymmetricExcept(IEnumerable<T> other);

    bool Overlaps(IEnumerable<T> other);

    bool IsSubsetOf(IEnumerable<T> other);

    bool IsProperSubsetOf(IEnumerable<T> other);

    bool IsSupersetOf(IEnumerable<T> other);

    bool IsProperSupersetOf(IEnumerable<T> other);

    IImmutableSet<T> AsImmutableSet();
}
