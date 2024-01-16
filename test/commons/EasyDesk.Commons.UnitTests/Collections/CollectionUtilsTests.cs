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

    public static TheoryData<ICollection<int>, Func<int, bool>, IEnumerable<int>> RemoveWhereCollectionStateData() => new()
    {
        { Of(), x => true, Of() },
        { Of(1, 2, 3), x => x > 4, Of(1, 2, 3) },
        { Of(1, 2, 3), x => x < 4, Of() },
        { Of(1, 2, 3), x => x % 2 == 0, Of(1, 3) },
    };

    [Theory]
    [MemberData(nameof(RemoveWhereRemovedItemsData))]
    public void RemoveWhere_ShouldReturnAllRemovedItems(ICollection<int> collection, Func<int, bool> predicate, IEnumerable<int> expected)
    {
        collection.RemoveWhere(predicate).ShouldBe(expected, ignoreOrder: true);
    }

    public static TheoryData<ICollection<int>, Func<int, bool>, IEnumerable<int>> RemoveWhereRemovedItemsData() => new()
    {
        { Of(), x => true, Of() },
        { Of(1, 2, 3), x => x > 4, Of() },
        { Of(1, 2, 3), x => x < 4, Of(1, 2, 3) },
        { Of(1, 2, 3), x => x % 2 == 0, Of(2) },
    };

    private static ICollection<int> Of(params int[] items) => new List<int>(items);
}
