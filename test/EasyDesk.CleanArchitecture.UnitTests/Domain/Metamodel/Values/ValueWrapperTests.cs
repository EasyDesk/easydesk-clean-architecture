using EasyDesk.CleanArchitecture.Domain.Metamodel.Values;
using Shouldly;
using System;
using Xunit;

namespace EasyDesk.CleanArchitecture.UnitTests.Domain.Metamodel.Values
{
    public class ValueWrapperTests
    {
        private static readonly int _innerValue = 5;

        private record TestValueWrapper : ValueWrapper<int>
        {
            public TestValueWrapper(int value) : base(value)
            {
            }

            protected override void Validate(int value)
            {
                if (value == 0)
                {
                    throw new ArgumentException("Value should not be 0.", nameof(value));
                }
            }

            public int InnerValue => Value;
        }

        private readonly TestValueWrapper _sut = new(_innerValue);

        [Fact]
        public void Validate_ShouldThrowException_WithWrongValue()
        {
            Should.Throw<ArgumentException>(() => new TestValueWrapper(0));
        }

        [Fact]
        public void WrappedValue_ShouldBeAccessible()
        {
            _sut.InnerValue.ShouldBe(_innerValue);
        }

        [Fact]
        public void Wrapper_ShouldBeUsedAsWrapped()
        {
            (_sut * 2).ShouldBe(_innerValue * 2);
        }
    }
}
