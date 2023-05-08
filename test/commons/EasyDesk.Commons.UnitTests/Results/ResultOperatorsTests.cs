using EasyDesk.Commons.Collections;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Shouldly;
using Xunit;

namespace EasyDesk.Commons.UnitTests.Results;

public class ResultOperatorsTests
{
    private const string TestString = "TEST";
    private const int Value = 10;

    private static readonly Error _error = new TestError(false);
    private static readonly Error _differentError = new TestError(true);

    private static Result<int> Success => Success(Value);

    private static Result<int> Failure => Failure<int>(_error);

    public static IEnumerable<object[]> AllTypesOfResult()
    {
        yield return new object[] { Success };
        yield return new object[] { Failure };
    }

    public static IEnumerable<object[]> FlatMapData()
    {
        yield return new object[] { Success(TestString) };
        yield return new object[] { Failure<string>(_differentError) };
    }

    [Fact]
    public void IfSuccess_ShouldNotCallTheGivenFunction_ForFailedResults()
    {
        var shouldNotBeCalled = Substitute.For<Action<int>>();

        Failure.IfSuccess(shouldNotBeCalled);

        shouldNotBeCalled.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public void IfSuccess_ShouldCallTheGivenFunction_ForSuccessfulResults()
    {
        var shouldBeCalled = Substitute.For<Action<int>>();

        Success.IfSuccess(shouldBeCalled);

        shouldBeCalled.Received(1)(Value);
    }

    [Theory]
    [MemberData(nameof(AllTypesOfResult))]
    public void IfSuccess_ShouldReturnTheSameResult(Result<int> result)
    {
        var output = result.IfSuccess(Substitute.For<Action<int>>());

        output.ShouldBe(result);
    }

    [Fact]
    public void IfFailure_ShouldCallTheGivenFunction_ForFailedResults()
    {
        var shouldBeCalled = Substitute.For<Action<Error>>();

        Failure.IfFailure(shouldBeCalled);

        shouldBeCalled.Received(1)(_error);
    }

    [Fact]
    public void IfFailure_ShouldNotCallTheGivenFunction_ForFailedResults()
    {
        var shouldNotBeCalled = Substitute.For<Action<Error>>();

        Success.IfFailure(shouldNotBeCalled);

        shouldNotBeCalled.DidNotReceiveWithAnyArgs()(default!);
    }

    [Theory]
    [MemberData(nameof(AllTypesOfResult))]
    public void IfFailure_ShouldReturnTheSameResult(Result<int> result)
    {
        var output = result.IfFailure(Substitute.For<Action<Error>>());

        output.ShouldBe(result);
    }

    [Fact]
    public void Map_ShouldMapTheWrappedValue_ForSuccessfulResults()
    {
        var mapper = Substitute.For<Func<int, string>>();
        mapper(Value).Returns(TestString);

        var output = Success.Map(mapper);

        output.ShouldBe(Success(TestString));
        mapper.Received(1)(Value);
    }

    [Fact]
    public void Map_ShouldReturnTheSameError_ForFailedResults()
    {
        var mapper = Substitute.For<Func<int, string>>();

        var output = Failure.Map(mapper);

        output.ShouldBe(Failure<string>(_error));
        mapper.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public void IgnoreResult_ShouldReturnNothing_ForSuccessfulResults()
    {
        var output = Success.IgnoreResult();

        output.ShouldBe(Success(Nothing.Value));
    }

    [Fact]
    public void IgnoreResult_ShouldReturnTheSameError_ForFailedResults()
    {
        var output = Failure.IgnoreResult();

        output.ShouldBe(Failure<string>(_error));
    }

    [Fact]
    public void MapError_ShouldMapTheWrappedError_ForFailedResults()
    {
        var mapper = Substitute.For<Func<Error, Error>>();
        mapper(_error).Returns(_differentError);

        var output = Failure.MapError(mapper);

        output.ShouldBe(Failure<int>(_differentError));
        mapper.Received(1)(_error);
    }

    [Fact]
    public void MapError_ShouldReturnTheSameValue_ForSuccessfulResults()
    {
        var mapper = Substitute.For<Func<Error, Error>>();

        var output = Success.MapError(mapper);

        output.ShouldBe(Success);
        mapper.DidNotReceiveWithAnyArgs()(default!);
    }

    [Fact]
    public void FlatMap_ShouldReturnTheSameError_ForFailedResults()
    {
        var mapper = Substitute.For<Func<int, Result<string>>>();

        var output = Failure.FlatMap(mapper);

        output.ShouldBe(Failure<string>(_error));
        mapper.DidNotReceiveWithAnyArgs()(Value);
    }

    [Theory]
    [MemberData(nameof(FlatMapData))]
    public void FlatMap_ShouldReturnTheMappedValue_ForSuccessfulResults(
        Result<string> mappedResult)
    {
        var mapper = Substitute.For<Func<int, Result<string>>>();
        mapper(Value).Returns(mappedResult);

        var output = Success.FlatMap(mapper);

        output.ShouldBe(mappedResult);
        mapper.Received(1)(Value);
    }

    [Fact]
    public void FlatTap_ShouldReturnTheSameError_ForFailedResults()
    {
        var mapper = Substitute.For<Func<int, Result<string>>>();

        var output = Failure.FlatTap(mapper);

        output.ShouldBe(Failure);
        mapper.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public void FlatTap_ShouldReturnTheMappedError_ForSuccessfulResults()
    {
        var mapper = Substitute.For<Func<int, Result<string>>>();
        mapper(Value).Returns(Failure<string>(_differentError));

        var output = Success.FlatTap(mapper);

        output.ShouldBe(Failure<int>(_differentError));
        mapper.Received(1)(Value);
    }

    [Fact]
    public void FlatTap_ShouldReturnTheMappedValue_ForSuccessfulResults()
    {
        var mapper = Substitute.For<Func<int, Result<string>>>();
        mapper(Value).Returns(Success(TestString));

        var output = Success.FlatTap(mapper);

        output.ShouldBe(Success);
        mapper.Received(1)(Value);
    }

    [Fact]
    public void Filter_ShouldReturnTheOriginalValue_IfPredicateIsTrue()
    {
        Success.Filter(x => x > 0, _ => _differentError).ShouldBe(Success);
    }

    [Fact]
    public void Filter_ShouldReturnTheOriginalError()
    {
        Failure.Filter(x => true, _ => _differentError).ShouldBe(_error);
    }

    [Fact]
    public void Filter_ShouldReturnTheMappedError_IfPredicateIsFalse()
    {
        Success.Filter(x => x < 0, _ => _differentError).ShouldBe(_differentError);
    }

    [Fact]
    public async Task IfSuccessAsync_ShouldNotCallTheGivenFunction_ForFailedResults()
    {
        var shouldNotBeCalled = Substitute.For<AsyncAction<int>>();

        await Failure.IfSuccessAsync(shouldNotBeCalled);

        await shouldNotBeCalled.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public async Task IfSuccessAsync_ShouldCallTheGivenFunction_ForSuccessfulResults()
    {
        var shouldBeCalled = Substitute.For<AsyncAction<int>>();

        await Success.IfSuccessAsync(shouldBeCalled);

        await shouldBeCalled.Received(1)(Value);
    }

    [Theory]
    [MemberData(nameof(AllTypesOfResult))]
    public async Task IfSuccessAsync_ShouldReturnTheSameResult(Result<int> result)
    {
        var output = await result.IfSuccessAsync(Substitute.For<AsyncAction<int>>());

        output.ShouldBe(result);
    }

    [Fact]
    public async Task IfFailureAsync_ShouldCallTheGivenFunction_ForFailedResults()
    {
        var shouldBeCalled = Substitute.For<AsyncAction<Error>>();

        await Failure.IfFailureAsync(shouldBeCalled);

        await shouldBeCalled.Received(1)(_error);
    }

    [Fact]
    public async Task IfFailureAsync_ShouldNotCallTheGivenFunction_ForFailedResults()
    {
        var shouldNotBeCalled = Substitute.For<AsyncAction<Error>>();

        await Success.IfFailureAsync(shouldNotBeCalled);

        await shouldNotBeCalled.DidNotReceiveWithAnyArgs()(default!);
    }

    [Theory]
    [MemberData(nameof(AllTypesOfResult))]
    public async Task IfFailureAsync_ShouldReturnTheSameResult(Result<int> result)
    {
        var output = await result.IfFailureAsync(Substitute.For<AsyncAction<Error>>());

        output.ShouldBe(result);
    }

    [Fact]
    public async Task MapAsync_ShouldMapTheWrappedValue_ForSuccessfulResults()
    {
        var mapper = Substitute.For<AsyncFunc<int, string>>();
        mapper(Value).Returns(TestString);

        var output = await Success.MapAsync(mapper);

        output.ShouldBe(Success(TestString));
        await mapper.Received(1)(Value);
    }

    [Fact]
    public async Task MapAsync_ShouldReturnTheSameError_ForFailedResults()
    {
        var mapper = Substitute.For<AsyncFunc<int, string>>();

        var output = await Failure.MapAsync(mapper);

        output.ShouldBe(Failure<string>(_error));
        await mapper.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public async Task FlatMapAsync_ShouldReturnTheSameError_ForFailedResults()
    {
        var mapper = Substitute.For<AsyncFunc<int, Result<string>>>();

        var output = await Failure.FlatMapAsync(mapper);

        output.ShouldBe(Failure<string>(_error));
        await mapper.DidNotReceiveWithAnyArgs()(Value);
    }

    [Theory]
    [MemberData(nameof(FlatMapData))]
    public async Task FlatMapAsync_ShouldReturnTheMappedValue_ForSuccessfulResults(
        Result<string> mappedResult)
    {
        var mapper = Substitute.For<AsyncFunc<int, Result<string>>>();
        mapper(Value).Returns(mappedResult);

        var output = await Success.FlatMapAsync(mapper);

        output.ShouldBe(mappedResult);
        await mapper.Received(1)(Value);
    }

    [Fact]
    public async Task FlatTapAsync_ShouldReturnTheSameError_ForFailedResults()
    {
        var mapper = Substitute.For<AsyncFunc<int, Result<string>>>();

        var output = await Failure.FlatTapAsync(mapper);

        output.ShouldBe(Failure);
        await mapper.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public async Task FlatTapAsync_ShouldReturnTheMappedError_ForSuccessfulResults()
    {
        var mapper = Substitute.For<AsyncFunc<int, Result<string>>>();
        mapper(Value).Returns(Failure<string>(_differentError));

        var output = await Success.FlatTapAsync(mapper);

        output.ShouldBe(Failure<int>(_differentError));
        await mapper.Received(1)(Value);
    }

    [Fact]
    public async Task FlatTapAsync_ShouldReturnTheMappedValue_ForSuccessfulResults()
    {
        var mapper = Substitute.For<AsyncFunc<int, Result<string>>>();
        mapper(Value).Returns(Success(TestString));

        var output = await Success.FlatTapAsync(mapper);

        output.ShouldBe(Success);
        await mapper.Received(1)(Value);
    }

    [Fact]
    public async Task FilterAsync_ShouldReturnTheOriginalValue_IfPredicateIsTrue()
    {
        var output = await Success.FilterAsync(x => Task.FromResult(x > 0), _ => _differentError);
        output.ShouldBe(Success);
    }

    [Fact]
    public async Task FilterAsync_ShouldReturnTheOriginalError()
    {
        var output = await Failure.FilterAsync(x => Task.FromResult(true), _ => _differentError);
        output.ShouldBe(_error);
    }

    [Fact]
    public async Task FilterAsync_ShouldReturnTheMappedError_IfPredicateIsFalse()
    {
        var output = await Success.FilterAsync(x => Task.FromResult(x < 0), _ => _differentError);
        output.ShouldBe(_differentError);
    }

    [Fact]
    public void ThrowIfFailure_ShouldNotThrowAnyException_ForSuccessfulResults()
    {
        Should.NotThrow(() => Success.ThrowIfFailure());
    }

    [Fact]
    public void ThrowIfFailure_ShouldReturnTheWrappedValue_ForSuccessfulResults()
    {
        Success.ThrowIfFailure().ShouldBe(Value);
    }

    [Fact]
    public void ThrowIfFailure_ShouldThrowAResultFailedException_ForFailedResults()
    {
        var exception = Should.Throw<ResultFailedException>(() => Failure.ThrowIfFailure());

        exception.Error.ShouldBe(_error);
    }

    [Fact]
    public void ThrowIfFailure_ShouldThrowTheGivenException_ForFailedResults()
    {
        var exceptionFactory = Substitute.For<Func<Error, Exception>>();
        var expectedException = new Exception("Test");
        exceptionFactory(_error).Returns(expectedException);

        var actualException = Should.Throw<Exception>(() => Failure.ThrowIfFailure(exceptionFactory));

        actualException.ShouldBe(expectedException);
        exceptionFactory.Received(1)(_error);
    }

    [Fact]
    public void Contains_ShouldReturnTrue_IfPropositionIsTrue_WithSuccess()
    {
        Success.Contains(_ => true).ShouldBeTrue();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_IfPropositionIsFalse_WithSuccess()
    {
        Success.Contains(_ => false).ShouldBeFalse();
    }

    [Fact]
    public void Contains_ShouldCallPredicate_Once()
    {
        var predicate = Substitute.For<Func<int, bool>>();
        Success.Contains(predicate);
        predicate.Received(1).Invoke(Value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Contains_ShouldReturnFalse_WithFailure(bool proposition)
    {
        Failure.Contains(_ => proposition).ShouldBeFalse();
    }

    [Fact]
    public void CatchFirstFailure_ShouldReturnOnlyTheFirstFailure_IfMultipleArePresent()
    {
        Enumerable.Range(0, 100)
            .Select(i => i switch
            {
                0 => _error,
                < 15 => Success(i),
                < 60 => _differentError,
                < 85 => Success(-i),
                _ => _differentError
            })
            .CatchFirstFailure()
            .ShouldBe(Failure<IEnumerable<int>>(_error));
    }

    [Fact]
    public void CatchFirstFailure_ShouldReturnSuccess_IfAllElementsAreSuccesses()
    {
        var enumerable = Enumerable.Range(0, 120);

        var result = enumerable
            .Select(Success<int>)
            .CatchFirstFailure();
        result.IsSuccess.ShouldBeTrue();
        result.ReadValue().ShouldBe(enumerable);
    }

    [Fact]
    public void CatchFirstFailure_ShouldReturnEmpty_WithEmptyInput()
    {
        var result = Enumerable.Empty<Result<Nothing>>().CatchFirstFailure();
        result.IsSuccess.ShouldBeTrue();
        result.ReadValue().ShouldBeEmpty();
    }

    [Fact]
    public void CatchAllFailures_ShouldReturnEveryFailure_IfMultipleArePresent()
    {
        var results = Enumerable.Range(0, 100)
            .Select(i => i switch
            {
                0 => _error,
                < 15 => Success(i),
                < 60 => _differentError,
                < 85 => Success(-i),
                _ => _differentError
            });
        var failures = results.Where(r => r.IsFailure).Select(e => e.ReadError());
        var result = results.CatchAllFailures();
        result.IsFailure.ShouldBeTrue();
        var multiError = result.ReadError().ShouldBeOfType<MultiError>();
        multiError.PrimaryError.ShouldBe(_error);
        multiError.SecondaryErrors.ShouldBe(failures.Skip(1));
    }

    [Fact]
    public void CatchAllFailures_ShouldReturnSuccess_IfAllElementsAreSuccesses()
    {
        var enumerable = Enumerable.Range(0, 120);

        var result = enumerable
            .Select(Success<int>)
            .CatchAllFailures();
        result.IsSuccess.ShouldBeTrue();
        result.ReadValue().ShouldBe(enumerable);
    }

    [Fact]
    public void CatchAllFailures_ShouldReturnEmpty_WithEmptyInput()
    {
        var result = Enumerable.Empty<Result<Nothing>>().CatchAllFailures();
        result.IsSuccess.ShouldBeTrue();
        result.ReadValue().ShouldBeEmpty();
    }
}
