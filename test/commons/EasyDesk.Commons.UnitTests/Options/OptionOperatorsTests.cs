using NSubstitute;
using Shouldly;
using Xunit;

namespace EasyDesk.Commons.UnitTests.Options;

public class OptionOperatorsTests
{
    private const int Value = 5;
    private const int Other = 10;

    [Fact]
    public void IfPresent_ShouldNotCallTheGivenAction_IfOptionIsEmpty()
    {
        var action = Substitute.For<Action<int>>();

        NoneT<int>().IfPresent(action);

        action.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public void IfPresent_ShouldCallTheGivenAction_IfOptionIsNotEmpty()
    {
        var action = Substitute.For<Action<int>>();

        Some(Value).IfPresent(action);

        action.Received(1)(Value);
    }

    [Fact]
    public void IfAbsent_ShouldCallTheGivenAction_IfOptionIsEmpty()
    {
        var action = Substitute.For<Action>();

        NoneT<int>().IfAbsent(action);

        action.Received(1)();
    }

    [Fact]
    public void IfAbsent_ShouldNotCallTheGivenAction_IfOptionIsNotEmpty()
    {
        var action = Substitute.For<Action>();

        Some(Value).IfAbsent(action);

        action.DidNotReceiveWithAnyArgs()();
    }

    [Fact]
    public void Map_ShouldReturnNone_IfOptionIsEmpty()
    {
        NoneT<int>().Map(x => x + 1).ShouldBe(None);
    }

    [Fact]
    public void Map_ShouldReturnTheMappedValue_IfOptionIsNotEmpty()
    {
        Some(Value).Map(x => x + 1).ShouldBe(Some(Value + 1));
    }

    [Fact]
    public void Filter_ShouldReturnNone_IfOptionIsEmpty()
    {
        NoneT<int>().Filter(_ => true).ShouldBe(None);
    }

    [Fact]
    public void Filter_ShouldReturnNone_IfOptionIsNotEmptyButPredicateIsNotMatched()
    {
        Some(Value).Filter(x => x != Value).ShouldBe(None);
    }

    [Fact]
    public void Filter_ShouldReturnTheSameOption_IfOptionIsNotEmptyAndPredicateIsMatched()
    {
        var some = Some(Value);
        some.Filter(x => x == Value).ShouldBe(some);
    }

    [Fact]
    public void FlatMap_ShouldReturnNone_IfOptionIsEmpty()
    {
        NoneT<int>().FlatMap(_ => Some(Value)).ShouldBe(None);
    }

    [Fact]
    public void FlatMap_ShouldReturnTheResultOfTheMapper_IfOptionIsNotEmpty()
    {
        Some(Value).FlatMap(_ => NoneT<int>()).ShouldBe(None);
        Some(Value).FlatMap(v => Some(v + 1)).ShouldBe(Some(Value + 1));
    }

    [Fact]
    public void Flatten_ShouldReturnNone_IfAnyOptionIsNone()
    {
        Some(NoneT<int>()).Flatten().ShouldBe(None);
        NoneT<Option<int>>().Flatten().ShouldBe(None);
    }

    [Fact]
    public void Flatten_ShouldReturnTheInnermostValue_IfBothOptionsAreNotEmpty()
    {
        Some(Some(Value)).Flatten().ShouldBe(Some(Value));
    }

    [Fact]
    public void Or_ShouldShortCircuit_IfTheFirstOptionIsNotEmpty()
    {
        var first = Some(Value);
        first.Or(NoneT<int>()).ShouldBe(first);
        first.Or(Some(Other)).ShouldBe(first);
    }

    [Fact]
    public void Or_ShouldReturnTheSecondOption_IfTheFirstIsEmpty()
    {
        NoneT<int>().Or(Some(Value)).ShouldBe(Some(Value));
        NoneT<int>().Or(NoneT<int>()).ShouldBe(None);
    }

    [Fact]
    public async Task IfPresentAsync_ShouldNotCallTheGivenAsyncAction_IfOptionIsEmpty()
    {
        var action = Substitute.For<AsyncAction<int>>();

        await NoneT<int>().IfPresentAsync(action);

        await action.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public async Task IfPresentAsync_ShouldCallTheGivenAsyncAction_IfOptionIsNotEmpty()
    {
        var action = Substitute.For<AsyncAction<int>>();

        await Some(Value).IfPresentAsync(action);

        await action.Received(1)(Value);
    }

    [Fact]
    public async Task IfAbsentAsync_ShouldCallTheGivenAsyncAction_IfOptionIsEmpty()
    {
        var action = Substitute.For<AsyncAction>();

        await NoneT<int>().IfAbsentAsync(action);

        await action.Received(1)();
    }

    [Fact]
    public async Task IfAbsentAsync_ShouldNotCallTheGivenAsyncAction_IfOptionIsNotEmpty()
    {
        var action = Substitute.For<AsyncAction>();

        await Some(Value).IfAbsentAsync(action);

        await action.DidNotReceiveWithAnyArgs()();
    }

    [Fact]
    public async Task MapAsync_ShouldReturnNone_IfOptionIsEmpty()
    {
        var result = await NoneT<int>().MapAsync(x => Task.FromResult(x + 1));

        result.ShouldBe(None);
    }

    [Fact]
    public async Task MapAsync_ShouldReturnTheMappedValue_IfOptionIsNotEmptyAsync()
    {
        var result = await Some(Value).MapAsync(x => Task.FromResult(x + 1));

        result.ShouldBe(Some(Value + 1));
    }

    [Fact]
    public async Task FilterAsync_ShouldReturnNone_IfOptionIsEmpty()
    {
        var result = await NoneT<int>().FilterAsync(_ => Task.FromResult(true));

        result.ShouldBe(None);
    }

    [Fact]
    public async Task FilterAsync_ShouldReturnNone_IfOptionIsNotEmptyButPredicateIsNotMatched()
    {
        var result = await Some(Value).FilterAsync(x => Task.FromResult(x != Value));

        result.ShouldBe(None);
    }

    [Fact]
    public async Task FilterAsync_ShouldReturnTheSameOption_IfOptionIsNotEmptyAndPredicateIsMatched()
    {
        var some = Some(Value);

        var result = await some.FilterAsync(x => Task.FromResult(x == Value));

        result.ShouldBe(some);
    }

    [Fact]
    public async Task FlatMapAsync_ShouldReturnNone_IfOptionIsEmpty()
    {
        var result = await NoneT<int>().FlatMapAsync(_ => Task.FromResult(Some(Value)));

        result.ShouldBe(None);
    }

    [Fact]
    public async Task FlatMapAsync_ShouldReturnNone_IfOptionIsNotEmptyAndMapperReturnsNone()
    {
        var result = await Some(Value).FlatMapAsync(_ => Task.FromResult(NoneT<int>()));

        result.ShouldBe(None);
    }

    [Fact]
    public async Task FlatMapAsync_ShouldReturnTheValueReturnedByTheGivenFunction_IfOptionIsNotEmpty()
    {
        var result = await Some(Value).FlatMapAsync(v => Task.FromResult(Some(v + 1)));

        result.ShouldBe(Some(Value + 1));
    }

    [Fact]
    public void Contains_ShouldReturnTrue_IfPropositionIsTrue_WithSome()
    {
        Some(Value).Contains(_ => true).ShouldBeTrue();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_IfPropositionIsFalse_WithSome()
    {
        Some(Value).Contains(_ => false).ShouldBeFalse();
    }

    [Fact]
    public void Contains_ShouldCallPredicate_Once()
    {
        var predicate = Substitute.For<Func<int, bool>>();
        Some(Value).Contains(predicate);
        predicate.Received(1).Invoke(Value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Contains_ShouldReturnFalse_WithFailure(bool proposition)
    {
        NoneT<int>().Contains(_ => proposition).ShouldBeFalse();
    }

    [Fact]
    public async Task ContainsAsync_ShouldReturnTrue_IfPropositionIsTrue_WithSome()
    {
        var result = await Some(Value).ContainsAsync(_ => Task.FromResult(true));
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ContainsAsync_ShouldReturnFalse_IfPropositionIsFalse_WithSome()
    {
        var result = await Some(Value).ContainsAsync(_ => Task.FromResult(false));
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ContainsAsync_ShouldCallPredicate_Once()
    {
        var predicate = Substitute.For<AsyncFunc<int, bool>>();
        await Some(Value).ContainsAsync(predicate);
        await predicate.Received(1).Invoke(Value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ContainsAsync_ShouldReturnFalse_WithFailure(bool proposition)
    {
        var result = await NoneT<int>().ContainsAsync(_ => Task.FromResult(proposition));
        result.ShouldBeFalse();
    }
}
