using EasyDesk.Commons.Collections.Immutable;
using Shouldly;
using Xunit;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.Commons.UnitTests.Collections.Immutable;

public class EquatableImmutableSetTests
{
    [Theory]
    [MemberData(nameof(EqualSetPairs))]
    public void Equals_ShouldReturnTrue_IfSetsContainTheSameElements(
        EquatableImmutableSet<int> left, EquatableImmutableSet<int> right)
    {
        left.Equals(right).ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(DifferentSetPairs))]
    public void Equals_ShouldReturnFalse_IfSetsContainDifferentElements(
        EquatableImmutableSet<int> left, EquatableImmutableSet<int> right)
    {
        left.Equals(right).ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(EqualSetPairs))]
    public void GetHashCode_ShouldReturnTheSameValue_ForEqualSets(
        EquatableImmutableSet<int> left, EquatableImmutableSet<int> right)
    {
        var hashLeft = left.GetHashCode();
        var hashRight = right.GetHashCode();

        (hashLeft == hashRight).ShouldBeTrue();
    }

    public static IEnumerable<object[]> EqualSetPairs()
    {
        yield return new object[] { Set<int>(), Set<int>() };
        yield return new object[] { Set(1, 2, 3), Set(1, 2, 3) };
        yield return new object[] { Set(1, 2, 3), Set(3, 2, 1) };
        var sameRefSet = Set(1, 2);
        yield return new object[] { sameRefSet, sameRefSet };
    }

    public static IEnumerable<object[]> DifferentSetPairs()
    {
        yield return new object[] { Set<int>(), Set(1) };
        yield return new object[] { Set(1, 2, 3), Set(1, 2, 3, 4) };
        yield return new object[] { Set(1, 2, 3), Set(1, 2) };
    }
}
