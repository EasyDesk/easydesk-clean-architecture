namespace EasyDesk.Commons.Comparers;

public enum OrderingDirection
{
    Ascending = 1,
    Descending = -1
}

public static class ComparisonUtils
{
    public static T Min<T>(params T[] values)
        where T : IComparable<T>
    {
        return values.Min()!;
    }

    public static T Max<T>(params T[] values)
        where T : IComparable<T>
    {
        return values.Max()!;
    }

    public static bool LessThan(int compareResult) => compareResult < 0;

    public static bool LessThanOrEqualTo(int compareResult) => compareResult <= 0;

    public static bool GreaterThan(int compareResult) => compareResult > 0;

    public static bool GreaterThanOrEqualTo(int compareresult) => compareresult >= 0;

    private static bool Is<T>(this IComparable<T> left, Func<int, bool> comparison, T right) =>
        comparison(left.CompareTo(right));

    public static bool IsLessThan<T>(this IComparable<T> left, T right) => left.Is(LessThan, right);

    public static bool IsLessThanOrEqualTo<T>(this IComparable<T> left, T right) => left.Is(LessThanOrEqualTo, right);

    public static bool IsGreaterThan<T>(this IComparable<T> left, T right) => left.Is(GreaterThan, right);

    public static bool IsGreaterThanOrEqualTo<T>(this IComparable<T> left, T right) => left.Is(GreaterThanOrEqualTo, right);

    public static IComparer<T> ComparerBy<T, P>(Func<T, P> property, OrderingDirection direction = OrderingDirection.Ascending)
        where P : IComparable<P> => Comparer<T>.Create(CompareBy(property, direction));

    public static IComparer<T> ThenBy<T, P>(this IComparer<T> comparer, Func<T, P> property, OrderingDirection direction = OrderingDirection.Ascending)
        where P : IComparable<P> =>
        Comparer<T>.Create((x, y) =>
        {
            var result = comparer.Compare(x, y);
            return result == 0 ? CompareBy(property, direction)(x, y) : result;
        });

    private static Comparison<T> CompareBy<T, P>(Func<T, P> property, OrderingDirection direction) where P : IComparable<P> =>
        (x, y) => property(x).CompareTo(property(y)) * (int)direction;
}
