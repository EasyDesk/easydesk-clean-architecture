using EasyDesk.Commons.Utils;
using EasyDesk.Testing.MatrixExpansion;
using Shouldly;
using static EasyDesk.Commons.Collections.EnumerableUtils;

namespace EasyDesk.Testing.UnitTests;

public class MatrixTests
{
    private readonly IEqualityComparer<object?[]> _comparer = EqualityComparers.From<object?[]>(
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
            .Build()
            .ToList();

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
            .Axis(2)
            .Build()
            .ToList();

        var expected = Items(
            new object[] { 1, 0, 0, 2 },
            new object[] { 0, 1, 0, 2 },
            new object[] { 0, 0, 1, 2 });

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }

    [Fact]
    public void EnumerableAxis()
    {
        var result = Matrix.Builder()
            .Axis(new int?[] { null })
            .Axis(new int?[] { null, null })
            .Build()
            .ToList();

        var expected = Items(
            new object?[] { null, null },
            new object?[] { null, null });

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }

    [Fact]
    public void BooleanAxis()
    {
        var result = Matrix.Builder()
            .BooleanAxis()
            .BooleanAxis()
            .Build()
            .ToList();

        var expected = Items(
            new object[] { true, true },
            new object[] { true, false },
            new object[] { false, true },
            new object[] { false, false });

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }

    [Fact]
    public void OptionAxis()
    {
        var result = Matrix.Builder()
            .OptionAxis<int>(new int[] { 1 })
            .OptionAxis(2)
            .Build()
            .ToList();

        var expected = Items(
            new object[] { Some(1), Some(2) },
            new object[] { Some(1), NoneT<int>() },
            new object[] { NoneT<int>(), Some(2) },
            new object[] { NoneT<int>(), NoneT<int>() });

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }

    private record AnError : Error;

    private record AnotherError : Error;

    [Fact]
    public void FailureResultAxis()
    {
        var result = Matrix.Builder()
            .ResultAxis<object>(builder => builder.Failures(new Error[] { new AnError(), new AnotherError() }))
            .ResultAxis<object>(builder => builder.Failure(new AnError()))
            .Build()
            .ToList();

        var expected = Items(
            new object[] { Failure<object>(new AnError()), Failure<object>(new AnError()) },
            new object[] { Failure<object>(new AnotherError()), Failure<object>(new AnError()) });

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }

    [Fact]
    public void ResultAxis()
    {
        var result = Matrix.Builder()
            .ResultAxis<Nothing>(builder => builder.Results(new Result<Nothing>[] { new AnError(), new AnotherError() }))
            .ResultAxis<int>(builder => builder.Result(Success(5)))
            .Build()
            .ToList();

        var expected = Items(
            new object[] { Failure<Nothing>(new AnError()), Success(5) },
            new object[] { Failure<Nothing>(new AnotherError()), Success(5) });

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }

    [Fact]
    public void SuccessAxis()
    {
        var result = Matrix.Builder()
            .ResultAxis<int>(builder => builder.Successes(new int[] { 1, 2 }))
            .ResultAxis<int>(builder => builder.Success(5))
            .ResultAxis<Nothing>(builder => builder.Success())
            .Build()
            .ToList();

        var expected = Items(
            new object[] { Success(1), Success(5), Ok },
            new object[] { Success(2), Success(5), Ok });

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }
}
