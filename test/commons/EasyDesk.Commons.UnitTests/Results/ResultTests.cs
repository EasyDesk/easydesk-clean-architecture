using EasyDesk.Testing.Errors;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EasyDesk.Commons.UnitTests.Results;

public class ResultTests
{
    private const string TestString = "TEST";

    private readonly int _value = 10;
    private readonly Error _error = TestError.Create();

    private Result<int> Success => Success(_value);

    private Result<int> Failure => Failure<int>(_error);

    [Fact]
    public void IsSuccess_ShouldBeTrue_OnlyForSuccessfulResults()
    {
        Success.IsSuccess.ShouldBeTrue();
        Failure.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    public void IsFailure_ShouldBeTrue_OnlyForFailedResults()
    {
        Success.IsFailure.ShouldBeFalse();
        Failure.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Value_ShouldBeNone_ForFailedResults()
    {
        Failure.Value.ShouldBeEmpty();
    }

    [Fact]
    public void Value_ShouldBeSome_ForSuccessfulResults()
    {
        Success.Value.ShouldContain(_value);
    }

    [Fact]
    public void Error_ShouldBeNone_ForSuccessfultResults()
    {
        Success.Error.ShouldBeEmpty();
    }

    [Fact]
    public void Error_ShouldBeSome_ForFailedResults()
    {
        Failure.Error.ShouldContain(_error);
    }

    [Fact]
    public void ReadValue_ShouldReturnTheWrappedValue_ForSuccessfulResults()
    {
        Success.ReadValue().ShouldBe(_value);
    }

    [Fact]
    public void ReadValue_ShouldThrow_ForFailedResults()
    {
        Should.Throw<InvalidOperationException>(() => Failure.ReadValue());
    }

    [Fact]
    public void ReadError_ShouldReturnTheWrappedError_ForFailedResults()
    {
        Failure.ReadError().ShouldBe(_error);
    }

    [Fact]
    public void ReadError_ShouldThrow_ForSuccessfulResults()
    {
        Should.Throw<InvalidOperationException>(Success.ReadError);
    }

    [Fact]
    public void Match_ShouldReturnTheSuccessBranch_ForSuccessfulResults()
    {
        var shouldBeCalled = Substitute.For<Func<int, string>>();
        shouldBeCalled(_value).Returns(TestString);
        var shouldNotBeCalled = Substitute.For<Func<Error, string>>();

        var result = Success.Match(
            success: shouldBeCalled,
            failure: shouldNotBeCalled);

        result.ShouldBe(TestString);
        shouldNotBeCalled.DidNotReceiveWithAnyArgs()(default!);
    }

    [Fact]
    public void Match_ShouldReturnTheFailureBranch_ForFailedResults()
    {
        var shouldNotBeCalled = Substitute.For<Func<int, string>>();
        var shouldBeCalled = Substitute.For<Func<Error, string>>();
        shouldBeCalled(_error).Returns(TestString);

        var result = Failure.Match(
            success: shouldNotBeCalled,
            failure: shouldBeCalled);

        result.ShouldBe(TestString);
        shouldNotBeCalled.DidNotReceiveWithAnyArgs()(default);
    }

    [Theory]
    [MemberData(nameof(AndOperatorData))]
    public void AndTests(Result<int> left, Result<int> right, Result<int> expected)
    {
        var result = left & right;
        result.ShouldBe(expected);
    }

    public static IEnumerable<object[]> AndOperatorData()
    {
        var success1 = Success(10);
        var success2 = Success(20);
        var failureA = Failure<int>(TestError.Create("A"));
        var failureB = Failure<int>(TestError.Create("B"));
        yield return new object[] { failureA, failureB, failureA };
        yield return new object[] { failureB, success2, failureB };
        yield return new object[] { success2, failureB, failureB };
        yield return new object[] { success1, success2, success2 };
    }

    [Theory]
    [MemberData(nameof(OrOperatorData))]
    public void OrTests(Result<int> left, Result<int> right, Result<int> expected)
    {
        var result = left | right;
        result.ShouldBe(expected);
    }

    public static IEnumerable<object[]> OrOperatorData()
    {
        var success1 = Success(10);
        var success2 = Success(20);
        var failureA = Failure<int>(TestError.Create("A"));
        var failureB = Failure<int>(TestError.Create("B"));
        yield return new object[] { failureA, failureB, failureB };
        yield return new object[] { failureB, success2, success2 };
        yield return new object[] { success2, failureB, success2 };
        yield return new object[] { success1, success2, success1 };
    }

    [Fact]
    public void ShortCircuitedAndOperatorShouldNotEvaluateTheSecondOperand_IfNotNecessary()
    {
        var shouldNotBeCalled = Substitute.For<Func<Result<int>>>();
        _ = Failure && shouldNotBeCalled();
        shouldNotBeCalled.DidNotReceiveWithAnyArgs()();
    }

    [Fact]
    public void ShortCircuitedOrOperatorShouldNotEvaluateTheSecondOperand_IfNotNecessary()
    {
        var shouldNotBeCalled = Substitute.For<Func<Result<int>>>();
        _ = Success || shouldNotBeCalled();
        shouldNotBeCalled.DidNotReceiveWithAnyArgs()();
    }
}
