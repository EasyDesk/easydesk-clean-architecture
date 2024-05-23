using EasyDesk.Commons.Collections.Immutable.Implementation;
using System.Collections.Immutable;

namespace EasyDesk.Commons.Collections.Immutable;

public static class FixedList
{
    public static IFixedList<T> Create<T>(ImmutableList<T> list) =>
        new ImmutableListAdapter<T>(list);

    public static IFixedList<T> FromSpan<T>(ReadOnlySpan<T> items) =>
        Create(ImmutableList.Create(items));
}
