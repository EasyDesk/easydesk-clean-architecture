using EasyDesk.Commons.Collections.Immutable.Implementation;
using System.Collections.Immutable;

namespace EasyDesk.Commons.Collections.Immutable;

public static class FixedHashSet
{
    public static IFixedSet<T> Create<T>(ImmutableHashSet<T> set) =>
        new ImmutableSetAdapter<T>(set, set.KeyComparer);

    public static IFixedSet<T> FromSpan<T>(ReadOnlySpan<T> items) =>
        Create(ImmutableHashSet.Create(items));
}
