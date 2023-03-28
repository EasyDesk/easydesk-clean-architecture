using NSubstitute;
using Shouldly;
using Xunit;
using static EasyDesk.Commons.Collections.EnumerableUtils;
using static System.Linq.Enumerable;

namespace EasyDesk.Commons.UnitTests.Collections;

public class EnumerableUtilsTests
{
    [Fact]
    public void Iterate_ShouldStartFromSeed()
    {
        Iterate(0, x => x + 1).First().ShouldBe(0);
    }

    [Fact]
    public void Iterate_ShouldUseTheGivenFunctionToComputeTheSequence()
    {
        var count = 10;
        Iterate(0, x => x + 1).Take(count).ShouldBe(Range(0, count));
    }

    [Fact]
    public void Generate_ShouldUseTheGivenSupplierEachTime()
    {
        var count = 10;
        var current = 0;
        Generate(() => current++).Take(count).ShouldBe(Range(0, count));
    }

    [Fact]
    public void IsEmpty_ShouldReturnTrue_IfTheSequenceContainsNoElements()
    {
        Empty<int>().IsEmpty().ShouldBeTrue();
    }

    [Fact]
    public void IsEmpty_ShouldReturnFalse_IfTheSequenceContainsAtLeastOneElement()
    {
        Items(1).IsEmpty().ShouldBeFalse();
    }

    [Fact]
    public void ForEach_ShouldCallTheGivenActionForEveryElementInTheSquence()
    {
        var count = 10;
        var action = Substitute.For<Action<int>>();
        var range = Range(0, count);

        range.ForEach(action);

        action.ReceivedWithAnyArgs(10)(default);
        Received.InOrder(() =>
        {
            foreach (var i in range)
            {
                action(i);
            }
        });
    }

    [Fact]
    public void IndexOf_ShouldReturnNone_IfNoItemsMatchThePredicate()
    {
        Items(1, 2, 3).IndexOf(x => x > 5).ShouldBeEmpty();
    }

    [Fact]
    public void IndexOf_ShouldReturnTheIndexOfTheFirstOccurrence()
    {
        Items(1, 2, 3, 4).IndexOf(x => x % 2 == 0).ShouldContain(1);
    }

    [Fact]
    public void LastIndexOf_ShouldReturnNone_IfNoItemsMatchThePredicate()
    {
        Items(1, 2, 3).LastIndexOf(x => x > 5).ShouldBeEmpty();
    }

    [Fact]
    public void LastIndexOf_ShouldReturnTheIndexOfTheLastOccurrence()
    {
        Items(1, 2, 3, 4).LastIndexOf(x => x % 2 == 0).ShouldContain(3);
    }

    [Theory]
    [MemberData(nameof(FirstOptionEmptyData))]
    public void FirstOption_ShouldReturnNone_IfNoItemsMatchThePredicate(
        IEnumerable<int> sequence, Func<int, bool> predicate)
    {
        sequence.FirstOption(predicate).ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(FirstOptionEmptyData))]
    public void LastOption_ShouldReturnNone_IfNoItemsMatchThePredicate(
        IEnumerable<int> sequence, Func<int, bool> predicate)
    {
        sequence.LastOption(predicate).ShouldBeEmpty();
    }

    public static IEnumerable<object[]> FirstOptionEmptyData()
    {
        yield return new object[] { Empty<int>(), new Func<int, bool>(_ => true) };
        yield return new object[] { Empty<int>(), new Func<int, bool>(_ => false) };
        yield return new object[] { Range(5, 10), new Func<int, bool>(x => x > 20) };
    }

    [Theory]
    [MemberData(nameof(FirstOptionNonEmptyData))]
    public void FirstOption_ShouldReturnTheFirstItemMatchingThePredicate_IfAny(
        IEnumerable<int> sequence, Func<int, bool> predicate, int expected)
    {
        sequence.FirstOption(predicate).ShouldContain(expected);
    }

    [Theory]
    [MemberData(nameof(LastOptionNonEmptyData))]
    public void LastOption_ShouldReturnTheLastItemMatchingThePredicate_IfAny(
        IEnumerable<int> sequence, Func<int, bool> predicate, int expected)
    {
        sequence.LastOption(predicate).ShouldContain(expected);
    }

    public static IEnumerable<object[]> FirstOptionNonEmptyData()
    {
        yield return new object[] { Range(5, 10), new Func<int, bool>(x => x > 3), 5 };
        yield return new object[] { Range(5, 10), new Func<int, bool>(x => x > 7), 8 };
    }

    public static IEnumerable<object[]> LastOptionNonEmptyData()
    {
        yield return new object[] { Range(5, 10), new Func<int, bool>(x => x <= 5), 5 };
        yield return new object[] { Range(5, 10), new Func<int, bool>(x => x <= 8), 8 };
    }

    [Theory]
    [MemberData(nameof(SingleOptionWithOneMatchData))]
    public void SingleOption_ShouldReturnTheOnlyItemMatchingThePredicate_IfNoOtherItemsMatchThePredicate(
        IEnumerable<int> sequence, Func<int, bool> predicate, int expected)
    {
        sequence.SingleOption(predicate).ShouldContain(expected);
    }

    public static IEnumerable<object[]> SingleOptionWithOneMatchData()
    {
        yield return new object[] { Range(5, 1), new Func<int, bool>(_ => true), 5 };
        yield return new object[] { Range(5, 10), new Func<int, bool>(x => x is > 7 and < 9), 8 };
    }

    [Theory]
    [MemberData(nameof(SingleOptionWithNoMatchesData))]
    public void SingleOption_ShouldReturnNone_IfNoItemsMatchThePredicate(
        IEnumerable<int> sequence, Func<int, bool> predicate)
    {
        sequence.SingleOption(predicate).ShouldBeEmpty();
    }

    public static IEnumerable<object[]> SingleOptionWithNoMatchesData()
    {
        yield return new object[] { Empty<int>(), new Func<int, bool>(_ => true) };
        yield return new object[] { Empty<int>(), new Func<int, bool>(_ => false) };
        yield return new object[] { Range(5, 10), new Func<int, bool>(x => x < 3) };
    }

    [Theory]
    [MemberData(nameof(SingleOptionWithMoreThanOneMatchData))]
    public void SingleOption_ShouldFail_IfMoreThanOneItemMatchesThePredicate(
        IEnumerable<int> sequence, Func<int, bool> predicate)
    {
        Should.Throw<InvalidOperationException>(() => sequence.SingleOption(predicate));
    }

    private class CustomException : Exception
    {
    }

    [Theory]
    [MemberData(nameof(SingleOptionWithMoreThanOneMatchData))]
    public void SingleOption_ShouldFailWithCustomException_IfMoreThanOneItemMatchesThePredicate(
        IEnumerable<int> sequence, Func<int, bool> predicate)
    {
        Should.Throw<CustomException>(() => sequence.SingleOption(predicate, () => new CustomException()));
    }

    public static IEnumerable<object[]> SingleOptionWithMoreThanOneMatchData()
    {
        yield return new object[] { Range(5, 10), new Func<int, bool>(x => x > 3) };
    }

    [Theory]
    [MemberData(nameof(ConcatStringsData))]
    public void ConcatStrings_ShouldReturnACorrectConcatenationOfStrings(
        IEnumerable<int> sequence, string expected)
    {
        sequence.ConcatStrings(",", "[", "]").ShouldBe(expected);
    }

    public static IEnumerable<object[]> ConcatStringsData()
    {
        yield return new object[] { Empty<int>(), "[]" };
        yield return new object[] { Range(1, 1), "[1]" };
        yield return new object[] { Range(1, 4), "[1,2,3,4]" };
    }

    [Fact]
    public void Scan_ShouldReturnTheSeedAlone_IfSequenceIsEmpty()
    {
        var next = Substitute.For<Func<string, int, string>>();
        var seed = string.Empty;
        Empty<int>().Scan(seed, next).ShouldBe(Items(seed));
    }

    [Fact]
    public void Scan_ShouldReturnTheSequenceOfResultStartingFromTheSeed_IfSequenceIsNotEmpty()
    {
        Range(1, 5).Scan(string.Empty, (s, n) => s + n).ShouldBe(Items(
            string.Empty,
            "1",
            "12",
            "123",
            "1234",
            "12345"));
    }

    [Fact]
    public void ZipScan_ShouldReturnAnEmptySequence_IfSequenceIsEmpty()
    {
        var next = Substitute.For<Func<string, int, string>>();
        Empty<int>().ZipScan(string.Empty, next).ShouldBe(Empty<(int, string)>());
    }

    [Fact]
    public void ZipScan_ShouldReturnTheSequenceOfResultsPairedWithItems_IfSequenceIsNotEmpty()
    {
        Range(1, 5).ZipScan(string.Empty, (s, n) => s + n).ShouldBe(Items(
            (1, "1"),
            (2, "12"),
            (3, "123"),
            (4, "1234"),
            (5, "12345")));
    }

    [Fact]
    public void FoldLeft_ShouldReturnTheSeed_IfTheSequenceIsEmpty()
    {
        Empty<int>().FoldLeft(0, (a, b) => a + b).ShouldBe(0);
    }

    [Fact]
    public void FoldLeft_ShouldCombineElementsToTheLeft_IfTheSequnceIsNotEmpty()
    {
        Range(1, 5).FoldLeft(string.Empty, (s, n) => s + n).ShouldBe("12345");
    }

    [Fact]
    public void FoldRight_ShouldReturnTheSeed_IfTheSequenceIsEmpty()
    {
        Empty<int>().FoldRight(0, (a, b) => a + b).ShouldBe(0);
    }

    [Fact]
    public void FoldRight_ShouldCombineElementsToTheRight_IfTheSequnceIsNotEmpty()
    {
        Range(1, 5).FoldRight(string.Empty, (n, s) => s + n).ShouldBe("54321");
    }

    [Fact]
    public void MatchesTwoByTwo_ShouldReturnTrue_IfTheSequenceIsEmpty()
    {
        Empty<int>().MatchesTwoByTwo((a, b) => false).ShouldBeTrue();
    }

    [Fact]
    public void MatchesTwoByTwo_ShouldReturnTrue_IfTheSequenceContainsOneElement()
    {
        Items(1).MatchesTwoByTwo((a, b) => false).ShouldBeTrue();
    }

    [Fact]
    public void MatchesTwoByTwo_ShouldReturnTrue_IfThePredicateIsSatisfiedByAllItemPairsInTheSequence()
    {
        Range(1, 5).MatchesTwoByTwo((a, b) => a < b).ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(FailingMatchesTwoByTwo))]
    public void MatchesTwoByTwo_ShouldReturnFalse_IfThePredicateIsNotSatisfiedByAnyItemPairInTheSequence(
        IEnumerable<int> sequence)
    {
        sequence.MatchesTwoByTwo((a, b) => a < b).ShouldBeFalse();
    }

    public static IEnumerable<object[]> FailingMatchesTwoByTwo()
    {
        yield return new object[] { Items(1, 0, 1, 0) };
        yield return new object[] { Items(1, 2, 1, 0) };
        yield return new object[] { Items(1, 2, 3, 0) };
        yield return new object[] { Items(4, 3, 2, 1) };
        yield return new object[] { Items(4, 3, 4, 5) };
    }

    [Fact]
    public void MinMaxOption_ShouldReturnNone_IfSequenceIsEmpty()
    {
        Empty<int>().MaxOption().ShouldBeEmpty();
        Empty<int>().MinOption().ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(MinMaxData))]
    public void MinMaxOption_ShouldReturnMinMax_IfSequenceIsNotEmpty(
        IEnumerable<int> sequence, int min, int max)
    {
        sequence.MinOption().ShouldContain(min);
        sequence.MaxOption().ShouldContain(max);
    }

    public record Wrapper(int Value);

    [Fact]
    public void MinMaxByOption_ShouldReturnNone_IfSequenceIsEmpty()
    {
        Empty<Wrapper>().MaxByOption(x => x.Value).ShouldBeEmpty();
        Empty<Wrapper>().MinByOption(x => x.Value).ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(MinMaxData))]
    public void MinMaxByOption_ShouldReturnMinMax_IfSequenceIsNotEmpty(
        IEnumerable<int> sequence, int min, int max)
    {
        var wrapped = sequence.Select(x => new Wrapper(x));
        wrapped.MinByOption(x => x.Value).ShouldBe(Some(new Wrapper(min)));
        wrapped.MaxByOption(x => x.Value).ShouldBe(Some(new Wrapper(max)));
    }

    public static IEnumerable<object[]> MinMaxData()
    {
        yield return new object[] { Items(1), 1, 1 };
        yield return new object[] { Items(1, 2, 3, 4, 5), 1, 5 };
        yield return new object[] { Items(3, 2, 1, 5, 4), 1, 5 };
    }

    [Theory]
    [MemberData(nameof(FilterNotNullData))]
    public void WhereNotNull_ShouldKeepOnlyNotNullValues(
        IEnumerable<string?> sequence, IEnumerable<string> expected)
    {
        sequence.WhereNotNull().ShouldBe(expected);
    }

    public static IEnumerable<object[]> FilterNotNullData()
    {
        yield return new object[] { Items("a", null, "b", null, null), Items("a", "b") };
        yield return new object[] { Empty<string>(), Empty<string>() };
        yield return new object[] { Items<string?>(null, null, null), Empty<string>() };
    }
}
