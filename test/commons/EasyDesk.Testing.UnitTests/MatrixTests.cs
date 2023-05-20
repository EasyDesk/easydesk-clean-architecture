using EasyDesk.Commons.Utils;
using EasyDesk.Testing.MatrixExpansion;
using Shouldly;
using static EasyDesk.Commons.Collections.EnumerableUtils;

namespace EasyDesk.Testing.UnitTests;

public class MatrixTests
{
    private readonly IEqualityComparer<object[]> _comparer = EqualityComparers.From<object[]>(
        (a, b) => a.SequenceEqual(b),
        x => x.CombineHashCodes());

    [Fact]
    public void ShouldExpandSingleLevel()
    {
        var result = Matrix.Builder().Axis(1, 2, 3).Build();

        var expected = Items(
            new object[] { 1 },
            new object[] { 2 },
            new object[] { 3 });

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }

    [Fact]
    public void ShouldExpandMultipleLevels()
    {
        var result = Matrix.Builder()
            .Axis(0, 1)
            .Axis("a", "b")
            .Axis(true, false)
            .Build();

        var expected = Items(
            new object[] { 0, "a", true },
            new object[] { 0, "a", false },
            new object[] { 0, "b", true },
            new object[] { 0, "b", false },
            new object[] { 1, "a", true },
            new object[] { 1, "a", false },
            new object[] { 1, "b", true },
            new object[] { 1, "b", false });

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }

    [Fact]
    public void ShouldFilterAtDeepLevels()
    {
        var result = Matrix.Builder()
            .Axis(0, 1)
            .Axis(0, 1)
            .Axis(0, 1)
            .Filter(x => x.Cast<int>().Sum() == 1)
            .Build();

        var expected = Items(
            new object[] { 1, 0, 0 },
            new object[] { 0, 1, 0 },
            new object[] { 0, 0, 1 });

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }
}
