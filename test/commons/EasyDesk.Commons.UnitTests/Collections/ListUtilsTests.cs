using EasyDesk.Commons.Collections;
using Shouldly;
using System.Collections.Immutable;
using Xunit;

namespace EasyDesk.Commons.UnitTests.Collections;

public class ListUtilsTests
{
    private static readonly ImmutableList<int> _list = [1, 1000, 5, 7, 2, 2, 13, 20, 234, -16, -20];

    private readonly ImmutableList<int> _sortedList;

    public ListUtilsTests()
    {
        _sortedList = _list.Sort();
    }

    public static IEnumerable<object?[]> Needles()
    {
        var sorted = _list.Sort();
        for (var i = -20; i < 20; i++)
        {
            if (sorted.Contains(i))
            {
                yield return new object?[] { i, sorted.BinarySearch(i) };
            }
            else
            {
                yield return new object?[] { i, ~sorted.BinarySearch(i) };
            }
        }
    }

    [Theory]
    [MemberData(nameof(Needles))]
    public void BinarySearchPreviousTests(int needle, int closest)
    {
        var haystack = _sortedList;
        if (haystack[closest] != needle)
        {
            closest--;
        }
        closest = closest < 0 ? 0 : closest;
        haystack.BinarySearchItemOrPrevious(needle).ShouldBe(closest);
    }

    [Theory]
    [MemberData(nameof(Needles))]
    public void BinarySearchNextTests(int needle, int closest)
    {
        var haystack = _sortedList;
        closest = closest >= haystack.Count ? haystack.Count - 1 : closest;
        haystack.BinarySearchItemOrNext(needle).ShouldBe(closest);
    }
}
