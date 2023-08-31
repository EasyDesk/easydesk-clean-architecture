using System.Collections.Immutable;

namespace EasyDesk.Commons.Collections;

public static class ImmutableListUtils
{
    public static int BinarySearchItemOrPrevious<T>(this ImmutableList<T> list, T key)
    {
        var result = list.BinarySearch(key);
        if (result >= 0)
        {
            return result;
        }
        result = ~result - 1;
        return result >= 0 ? result : 0;
    }

    public static int BinarySearchItemOrNext<T>(this ImmutableList<T> list, T key)
    {
        var result = list.BinarySearch(key);
        if (result >= 0)
        {
            return result;
        }
        result = ~result;
        return result < list.Count ? result : list.Count - 1;
    }
}
