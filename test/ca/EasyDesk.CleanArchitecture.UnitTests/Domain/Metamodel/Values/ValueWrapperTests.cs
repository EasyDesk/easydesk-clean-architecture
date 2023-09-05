using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using FluentValidation;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Domain.Metamodel.Values;

public class ValueWrapperTests
{
    private static readonly int _innerValue = 5;

    private record TestValueWrapper : PureValue<int, TestValueWrapper>, IValue<int>
    {
        public TestValueWrapper(int value) : base(value)
        {
        }

        public static IRuleBuilder<X, int> Validate<X>(IRuleBuilder<X, int> rules) => rules.NotEqual(0);
    }

    private readonly TestValueWrapper _sut = new(_innerValue);

    [Fact]
    public void Validate_ShouldThrowException_WithWrongValue()
    {
        var exception = Should.Throw<ValidationException>(() => new TestValueWrapper(0));
        exception.Errors.ShouldHaveSingleItem();
    }

    [Fact]
    public void WrappedValue_ShouldBeAccessible()
    {
        _sut.Value.ShouldBe(_innerValue);
    }

    [Fact]
    public void Wrapper_ShouldBeUsedAsWrapped()
    {
        (_sut * 2).ShouldBe(_innerValue * 2);
    }

    [Fact]
    public void Wrapper_ShouldBeImplicitlyConvertibleToWrapped()
    {
        int value = _sut;
        value.ShouldBe(_innerValue);
    }
}
