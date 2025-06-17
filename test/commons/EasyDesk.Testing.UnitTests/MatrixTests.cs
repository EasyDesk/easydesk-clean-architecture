using EasyDesk.Commons.Comparers;
using EasyDesk.Commons.Results;
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

        var expected = Expected(
            [1],
            [2],
            [3]);

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

        var expected = Expected(
            [0, "a", true],
            [0, "a", false],
            [0, "b", true],
            [0, "b", false],
            [1, "a", true],
            [1, "a", false],
            [1, "b", true],
            [1, "b", false]);

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }

    [Fact]
    public void ShouldMergeWithFlatAxis()
    {
        var result = Matrix.Builder()
            .Axis(0, 1)
            .Axis("a", "b")
            .Axis(true, false)
            .FlatAxis(Matrix.Builder().Axis(0.1, 0.2).Axis(default(object)).Build())
            .Build()
            .ToList();

        var expected = Expected(
            [0, "a", true, 0.1, null],
            [0, "a", false, 0.1, null],
            [0, "b", true, 0.1, null],
            [0, "b", false, 0.1, null],
            [1, "a", true, 0.1, null],
            [1, "a", false, 0.1, null],
            [1, "b", true, 0.1, null],
            [1, "b", false, 0.1, null],
            [0, "a", true, 0.2, null],
            [0, "a", false, 0.2, null],
            [0, "b", true, 0.2, null],
            [0, "b", false, 0.2, null],
            [1, "a", true, 0.2, null],
            [1, "a", false, 0.2, null],
            [1, "b", true, 0.2, null],
            [1, "b", false, 0.2, null]);

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

        var expected = Expected(
            [1, 0, 0, 2],
            [0, 1, 0, 2],
            [0, 0, 1, 2]);

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }

    [Fact]
    public void ShouldFilterAtDeepLevelsUsingArrays()
    {
        var result = Matrix.Builder()
            .Axis(0, 1)
            .Axis(0, 1)
            .Axis(0, 1)
            .Filter(x => (int)x[0]! + (int)x[1]! + (int)x[2]! == 1)
            .Axis(2)
            .Build()
            .ToList();

        var expected = Expected(
            [1, 0, 0, 2],
            [0, 1, 0, 2],
            [0, 0, 1, 2]);

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }

    [Fact]
    public void EnumerableAxis()
    {
        var result = Matrix.Builder()
            .Axis(new int?[] { null, })
            .Axis(new int?[] { null, null, })
            .Build()
            .ToList();

        var expected = Expected(
            [null, null],
            [null, null]);

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

        var expected = Expected(
            [true, true],
            [true, false],
            [false, true],
            [false, false]);

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }

    [Fact]
    public void OptionAxis()
    {
        var result = Matrix.Builder()
            .OptionAxis(1)
            .OptionAxis(2)
            .Build()
            .ToList();

        var expected = Expected(
            [Some(1), Some(2)],
            [Some(1), NoneT<int>()],
            [NoneT<int>(), Some(2)],
            [NoneT<int>(), NoneT<int>()]);

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }

    private record AnError : Error;

    private record AnotherError : Error;

    [Fact]
    public void FailureResultAxis()
    {
        var result = Matrix.Builder()
            .ResultAxis<object>(builder => builder.Failures(new Error[] { new AnError(), new AnotherError(), }))
            .ResultAxis<object>(builder => builder.Failure(new AnError()))
            .Build()
            .ToList();

        var expected = Expected(
            [Failure<object>(new AnError()), Failure<object>(new AnError())],
            [Failure<object>(new AnotherError()), Failure<object>(new AnError())]);

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }

    [Fact]
    public void ResultAxis()
    {
        var result = Matrix.Builder()
            .ResultAxis<Nothing>(builder => builder.Results(new Result<Nothing>[] { new AnError(), new AnotherError(), }))
            .ResultAxis<int>(builder => builder.Result(Success(5)))
            .Build()
            .ToList();

        var expected = Expected(
            [Failure<Nothing>(new AnError()), Success(5)],
            [Failure<Nothing>(new AnotherError()), Success(5)]);

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }

    [Fact]
    public void SuccessAxis()
    {
        var result = Matrix.Builder()
            .ResultAxis<int>(builder => builder.Successes(1, 2))
            .ResultAxis<int>(builder => builder.Success(5))
            .ResultAxis<Nothing>(builder => builder.Success())
            .Build()
            .ToList();

        var expected = Expected(
            [Success(1), Success(5), Ok],
            [Success(2), Success(5), Ok]);

        result.ShouldBe(expected, _comparer, ignoreOrder: true);
    }

    private IEnumerable<object?[]> Expected(params object?[][] arrays)
    {
        return Items(arrays);
    }
}
