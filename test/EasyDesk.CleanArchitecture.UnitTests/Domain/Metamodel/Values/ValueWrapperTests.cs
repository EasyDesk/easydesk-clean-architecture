using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using EasyDesk.CleanArchitecture.Testing;
using Shouldly;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Domain.Metamodel.Values;

public class ValueWrapperTests
{
    private static readonly int _innerValue = 5;

    private record TestValueWrapper : ValueWrapper<int, TestValueWrapper>
    {
        public TestValueWrapper(int value) : base(value)
        {
            DomainConstraints.Check()
                .IfNot(value != 0, () => TestDomainError.Create())
                .ThrowException();
        }
    }

    private readonly TestValueWrapper _sut = new(_innerValue);

    [Fact]
    public void Validate_ShouldThrowException_WithWrongValue()
    {
        var exception = Should.Throw<DomainConstraintException>(() => new TestValueWrapper(0));
        exception.DomainErrors.ShouldHaveSingleItem().ShouldBe(TestDomainError.Create());
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
}
