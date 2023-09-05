using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using FluentValidation;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Domain.Metamodel.Values;

public class QuantityWrapperTests
{
    private static readonly int _innerValue = 5;

    private record TestQuantityWrapper : Quantity<int, TestQuantityWrapper>, IValue<int>
    {
        public TestQuantityWrapper(int value) : base(value)
        {
        }

        public int Value => InnerValue;

        public static IRuleBuilder<X, int> Validate<X>(IRuleBuilder<X, int> rules) => rules.NotEqual(0);
    }

    private readonly TestQuantityWrapper _sut = new(_innerValue);

    [Fact]
    public void Validate_ShouldThrowException_WithWrongValue()
    {
        var exception = Should.Throw<ValidationException>(() => new TestQuantityWrapper(0));
        exception.Errors.ShouldHaveSingleItem();
    }

    [Fact]
    public void WrappedValue_ShouldBeAccessible()
    {
        _sut.Value.ShouldBe(_innerValue);
    }

    [Fact]
    public void QuantityWrapper_ShouldBeComparable()
    {
        var other = new TestQuantityWrapper(_innerValue);
        _sut.CompareTo(other).ShouldBe(0);
        (_sut <= other && _sut >= other).ShouldBeTrue();
        (_sut < other || _sut > other).ShouldBeFalse();
        other = new(10000000);
        _sut.CompareTo(other).ShouldBeLessThan(0);
        (_sut < other && _sut <= other).ShouldBeTrue();
        other = new(-100000000);
        _sut.CompareTo(other).ShouldBeGreaterThan(0);
        (_sut > other && _sut >= other).ShouldBeTrue();
    }
}
