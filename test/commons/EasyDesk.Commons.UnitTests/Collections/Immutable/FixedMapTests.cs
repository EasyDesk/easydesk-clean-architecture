using EasyDesk.Commons.Collections.Immutable;
using Shouldly;
using System.Collections.Immutable;
using Xunit;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.Commons.UnitTests.Collections.Immutable;

public class FixedMapTests
{
    [Theory]
    [MemberData(nameof(EqualDictionaryPairs))]
    public void Equals_ShouldReturnTrue_IfTheDictionariesContainTheSameKeyValuePairs(
        IFixedMap<int, string> left, IFixedMap<int, string> right)
    {
        left.Equals(right).ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(DifferentDictionaryPairs))]
    public void Equals_ShouldReturnFalse_IfTheDictionariesContainDifferentKeyValuePairs(
        IFixedMap<int, string> left, IFixedMap<int, string> right)
    {
        left.Equals(right).ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(EqualDictionaryPairs))]
    public void GetHashCode_ShouldReturnTheSameValue_ForEqualDictionaries(
        IFixedMap<int, string> left, IFixedMap<int, string> right)
    {
        var hashLeft = left.GetHashCode();
        var hashRight = right.GetHashCode();

        (hashLeft == hashRight).ShouldBeTrue();
    }

    public static TheoryData<IFixedMap<int, string>, IFixedMap<int, string>> EqualDictionaryPairs()
    {
        var sameRefMap = Map((1, "one"), (2, "two"));
        return new()
        {
            { Map<int, string>(), Map<int, string>() },
            { Map((1, "one"), (2, "two")), Map((2, "two"), (1, "one")) },
            { sameRefMap, sameRefMap },
        };
    }

    public static TheoryData<IFixedMap<int, string>, IFixedMap<int, string>> DifferentDictionaryPairs() => new()
    {
        { Map<int, string>(), Map((1, "one")) },
        { Map((1, "one"), (2, "two")), Map((1, "one")) },
        { Map((1, "one"), (2, "two")), Map((1, "one"), (2, "two"), (3, "three")) },
    };
}
