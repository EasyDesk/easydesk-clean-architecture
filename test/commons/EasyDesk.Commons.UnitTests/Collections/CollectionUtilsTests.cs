using EasyDesk.Commons.Collections;
using Shouldly;
using Xunit;

namespace EasyDesk.Commons.UnitTests.Collections;

public class CollectionUtilsTests
{
    [Theory]
    [MemberData(nameof(RemoveWhereCollectionStateData))]
    public void RemoveWhere_ShouldRemoveAllMatchingItems(ICollection<int> collection, Func<int, bool> predicate, IEnumerable<int> expected)
    {
        collection.RemoveWhere(predicate);
        collection.ShouldBe(expected, ignoreOrder: true);
    }

    public static IEnumerable<object[]> RemoveWhereCollectionStateData()
    {
        yield return new object[] { Of(), Predicate(x => true), Of() };
        yield return new object[] { Of(1, 2, 3), Predicate(x => x > 4), Of(1, 2, 3) };
        yield return new object[] { Of(1, 2, 3), Predicate(x => x < 4), Of() };
        yield return new object[] { Of(1, 2, 3), Predicate(x => x % 2 == 0), Of(1, 3) };
    }

    [Theory]
    [MemberData(nameof(RemoveWhereRemovedItemsData))]
    public void RemoveWhere_ShouldReturnAllRemovedItems(ICollection<int> collection, Func<int, bool> predicate, IEnumerable<int> expected)
    {
        collection.RemoveWhere(predicate).ShouldBe(expected, ignoreOrder: true);
    }

    public static IEnumerable<object[]> RemoveWhereRemovedItemsData()
    {
        yield return new object[] { Of(), Predicate(x => true), Of() };
        yield return new object[] { Of(1, 2, 3), Predicate(x => x > 4), Of() };
        yield return new object[] { Of(1, 2, 3), Predicate(x => x < 4), Of(1, 2, 3) };
        yield return new object[] { Of(1, 2, 3), Predicate(x => x % 2 == 0), Of(2) };
    }

    private static Func<int, bool> Predicate(Func<int, bool> f) => f;

    private static ICollection<int> Of(params int[] items) => new List<int>(items);
}
