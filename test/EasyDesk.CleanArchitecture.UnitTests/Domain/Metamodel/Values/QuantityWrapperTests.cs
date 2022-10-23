using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using EasyDesk.CleanArchitecture.Testing.Unit.Domain;
using Shouldly;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Domain.Metamodel.Values;

public class QuantityWrapperTests
{
    private static readonly int _innerValue = 5;

    private record TestQuantityWrapper : QuantityWrapper<int, TestQuantityWrapper>
    {
        public TestQuantityWrapper(int value) : base(value)
        {
            DomainConstraints.Check()
                .IfNot(value != 0, () => TestDomainError.Create())
                .ThrowException();
        }
    }

    private readonly TestQuantityWrapper _sut = new(_innerValue);

    [Fact]
    public void Validate_ShouldThrowException_WithWrongValue()
    {
        var exception = Should.Throw<DomainConstraintException>(() => new TestQuantityWrapper(0));
        exception.DomainErrors.ShouldHaveSingleItem().ShouldBe(TestDomainError.Create());
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
