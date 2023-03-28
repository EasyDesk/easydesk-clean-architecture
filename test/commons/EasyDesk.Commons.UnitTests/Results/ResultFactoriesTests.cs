using Shouldly;
using Xunit;

namespace EasyDesk.Commons.UnitTests.Results;

public class ResultFactoriesTests
{
    private readonly Error _error = new TestError(false);

    [Fact]
    public void OkShouldBeSuccessful()
    {
        Ok.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Ensure_ShouldReturnOk_IfTheConditionIsMet()
    {
        Ensure(true, otherwise: () => _error).ShouldBe(Ok);
    }

    [Fact]
    public void Ensure_ShouldReturnTheGivenError_IfTheConditionIsNotMet()
    {
        Ensure(false, otherwise: () => _error).ShouldBe(Failure<Nothing>(_error));
    }

    [Fact]
    public void EnsureNot_ShouldReturnOk_IfTheConditionIsNotMet()
    {
        EnsureNot(false, otherwise: () => _error).ShouldBe(Ok);
    }

    [Fact]
    public void EnsureNot_ShouldReturnTheGivenError_IfTheConditionIsMet()
    {
        EnsureNot(true, otherwise: () => _error).ShouldBe(Failure<Nothing>(_error));
    }

    [Fact]
    public void OrElseError_ShouldReturnSuccess_IfTheOptionIsNotEmpty()
    {
        Some(10).OrElseError(() => _error).ShouldBe(Success(10));
    }

    [Fact]
    public void OrElseError_ShouldReturnTheGivenError_IfTheOptionIsEmpty()
    {
        NoneT<int>().OrElseError(() => _error).ShouldBe(Failure<int>(_error));
    }

    [Fact]
    public void EnsureOfT_ShouldReturnSuccess_IfPredicateIsTrue()
    {
        Ensure(10, i => i > 0, _ => _error).ShouldBe(Success(10));
    }

    [Fact]
    public void EnsureOfT_ShouldReturnError_IfPredicateIsFalse()
    {
        Ensure(10, i => i < 0, _ => _error).ShouldBe(Failure<int>(_error));
    }
}
