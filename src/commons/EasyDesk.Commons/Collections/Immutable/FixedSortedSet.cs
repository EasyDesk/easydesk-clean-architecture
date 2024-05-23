using EasyDesk.Commons.Collections.Immutable.Implementation;
using EasyDesk.Commons.Comparers;
using System.Collections.Immutable;

namespace EasyDesk.Commons.Collections.Immutable;

public static class FixedSortedSet
{
    public static IFixedSet<T> Create<T>(ImmutableSortedSet<T> set) =>
        new ImmutableSetAdapter<T>(set, EqualityComparers.FromComparer(set.KeyComparer));

    public static IFixedSet<T> FromSpan<T>(ReadOnlySpan<T> items) =>
        Create(ImmutableSortedSet.Create(items));
}
