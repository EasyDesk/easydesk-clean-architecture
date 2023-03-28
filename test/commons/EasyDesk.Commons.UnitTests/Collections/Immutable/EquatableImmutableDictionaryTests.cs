using EasyDesk.Commons.Collections.Immutable;
using Shouldly;
using Xunit;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.Commons.UnitTests.Collections.Immutable;

public class EquatableImmutableDictionaryTests
{
    [Theory]
    [MemberData(nameof(EqualDictionaryPairs))]
    public void Equals_ShouldReturnTrue_IfTheDictionariesContainTheSameKeyValuePairs(
        EquatableImmutableDictionary<int, string> left, EquatableImmutableDictionary<int, string> right)
    {
        left.Equals(right).ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(DifferentDictionaryPairs))]
    public void Equals_ShouldReturnFalse_IfTheDictionariesContainDifferentKeyValuePairs(
        EquatableImmutableDictionary<int, string> left, EquatableImmutableDictionary<int, string> right)
    {
        left.Equals(right).ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(EqualDictionaryPairs))]
    public void GetHashCode_ShouldReturnTheSameValue_ForEqualDictionaries(
        EquatableImmutableDictionary<int, string> left, EquatableImmutableDictionary<int, string> right)
    {
        var hashLeft = left.GetHashCode();
        var hashRight = right.GetHashCode();

        (hashLeft == hashRight).ShouldBeTrue();
    }

    public static IEnumerable<object[]> EqualDictionaryPairs()
    {
        yield return new object[] { Map<int, string>(), Map<int, string>() };
        yield return new object[] { Map((1, "one"), (2, "two")), Map((2, "two"), (1, "one")) };

        var sameRefMap = Map((1, "one"), (2, "two"));
        yield return new object[] { sameRefMap, sameRefMap };
    }

    public static IEnumerable<object[]> DifferentDictionaryPairs()
    {
        yield return new object[] { Map<int, string>(), Map((1, "one")) };
        yield return new object[] { Map((1, "one"), (2, "two")), Map((1, "one")) };
        yield return new object[] { Map((1, "one"), (2, "two")), Map((1, "one"), (2, "two"), (3, "three")) };
    }
}
