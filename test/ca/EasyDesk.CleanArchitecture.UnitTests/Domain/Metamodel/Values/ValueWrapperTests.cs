using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using FluentValidation;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.Domain.Metamodel.Values;

public class ValueWrapperTests
{
    private const int InnerValue = 5;

    private class TestException : Exception
    {
        public TestException(string message) : base(message)
        {
        }
    }

    private record TestValueWrapper : PureValue<int, TestValueWrapper>, IValue<int>
    {
        public TestValueWrapper(int value) : base(value)
        {
        }

        public static IRuleBuilder<X, int> ValidationRules<X>(IRuleBuilder<X, int> rules) => rules
            .NotEqual(0)
            .NotEqual(-1000)
            .DependentRules(() => rules.Must(x => x != -1000 ? true : throw new TestException("This exception should almost never be thrown")));
    }

    private readonly TestValueWrapper _sut = new(InnerValue);

    [Fact]
    public void Validate_ShouldThrowException_WithWrongValue()
    {
        var exception = Should.Throw<ValidationException>(() => new TestValueWrapper(0));
        var error = exception.Errors.ShouldHaveSingleItem();
        error.PropertyName.ShouldBe(nameof(TestValueWrapper.Value));
    }

    [Fact]
    public void WrappedValue_ShouldBeAccessible()
    {
        _sut.Value.ShouldBe(InnerValue);
    }

    [Fact]
    public void Wrapper_ShouldBeUsedAsWrapped()
    {
        (_sut * 2).ShouldBe(InnerValue * 2);
    }

    [Fact]
    public void Wrapper_ShouldBeImplicitlyConvertibleToWrapped()
    {
        int value = _sut;
        value.ShouldBe(InnerValue);
    }

    [Fact]
    public void ShouldInvokeDependendRules_WhenConditionsAreSatisfied()
    {
        Should.Throw<TestException>(() => new TestValueWrapper(-1000));
    }
}
