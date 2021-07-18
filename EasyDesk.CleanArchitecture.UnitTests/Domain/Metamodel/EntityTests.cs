using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Testing.MatrixExpansion;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;
using static EasyDesk.Tools.Collections.EnumerableUtils;

namespace EasyDesk.CleanArchitecture.UnitTests.Domain.Metamodel
{
    public class EntityTests
    {
        public class TestEntity : Entity<TestEntity, int>.ExplicitId
        {
            public override int Id { get; }

            public int Value { get; }

            public TestEntity(int id, int value)
            {
                Id = id;
                Value = value;
            }
        }

        [Theory]
        [MemberData(nameof(EntitiesWithTheSameId))]
        public void Equals_ShouldReturnTrue_IfTheIdsAreEqual(
            TestEntity left, TestEntity right)
        {
            left.Equals(right).ShouldBeTrue();
        }

        public static IEnumerable<object[]> EntitiesWithTheSameId()
        {
            var id = 1;
            var v1 = 1;
            var v2 = 2;
            return Matrix
                .Fixed(new TestEntity(id, v1))
                .Axis(Items(new TestEntity(id, v1), new TestEntity(id, v2)))
                .Build();
        }

        [Theory]
        [MemberData(nameof(EntitiesWithDifferentIds))]
        public void Equals_ShouldReturnFalse_IfTheIdsAreNotEqual(
            TestEntity left, TestEntity right)
        {
            left.Equals(right).ShouldBeFalse();
        }

        public static IEnumerable<object[]> EntitiesWithDifferentIds()
        {
            var id1 = 1;
            var id2 = 2;
            var v = 2;
            return Matrix
                .Fixed(new TestEntity(id1, v))
                .Axis(Items(new TestEntity(id2, v), new TestEntity(id2, v)))
                .Build();
        }

        [Fact]
        public void GetHashCode_ShouldReturnTheHashCodeOfTheId()
        {
            var id = 1;
            new TestEntity(id, 5).GetHashCode().ShouldBe(HashCode.Combine(id));
        }
    }
}
