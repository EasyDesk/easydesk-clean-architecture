using Shouldly;
using System.Collections.Immutable;
using Xunit;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.Commons.UnitTests.Collections.Immutable;

public class EquatableImmutableSetTests
{
    [Theory]
    [MemberData(nameof(EqualSetPairs))]
    public void Equals_ShouldReturnTrue_IfSetsContainTheSameElements(
        IImmutableSet<int> left, IImmutableSet<int> right)
    {
        left.Equals(right).ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(DifferentSetPairs))]
    public void Equals_ShouldReturnFalse_IfSetsContainDifferentElements(
        IImmutableSet<int> left, IImmutableSet<int> right)
    {
        left.Equals(right).ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(EqualSetPairs))]
    public void GetHashCode_ShouldReturnTheSameValue_ForEqualSets(
        IImmutableSet<int> left, IImmutableSet<int> right)
    {
        var hashLeft = left.GetHashCode();
        var hashRight = right.GetHashCode();

        (hashLeft == hashRight).ShouldBeTrue();
    }

    public static TheoryData<IImmutableSet<int>, IImmutableSet<int>> EqualSetPairs()
    {
        var sameRefSet = Set(1, 2);
        return new()
        {
            { Set<int>(), Set<int>() },
            { Set(1, 2, 3), Set(1, 2, 3) },
            { Set(1, 2, 3), Set(3, 2, 1) },
            { sameRefSet, sameRefSet },
        };
    }

    public static TheoryData<IImmutableSet<int>, IImmutableSet<int>> DifferentSetPairs() => new()
    {
        { Set<int>(), Set(1) },
        { Set(1, 2, 3), Set(1, 2, 3, 4) },
        { Set(1, 2, 3), Set(1, 2) },
    };
}
