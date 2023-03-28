using NSubstitute;
using Shouldly;
using Xunit;

namespace EasyDesk.Commons.UnitTests.Options;

public class OptionTests
{
    private const int Value = 5;

    [Fact]
    public void IsPresent_ShouldBeTrue_OnlyIfOptionIsNotEmpty()
    {
        NoneT<int>().IsPresent.ShouldBeFalse();
        Some(Value).IsPresent.ShouldBeTrue();
    }

    [Fact]
    public void IsAbsent_ShouldBeTrue_OnlyIfOptionIsEmpty()
    {
        NoneT<int>().IsAbsent.ShouldBeTrue();
        Some(Value).IsAbsent.ShouldBeFalse();
    }

    [Fact]
    public void ReadingValue_ShouldFail_IfOptionIsEmpty()
    {
        Assert.Throws<InvalidOperationException>(() => NoneT<int>().Value);
    }

    [Fact]
    public void ReadingValue_ShouldSucceed_IfOptionIsNotEmpty()
    {
        Some(Value).Value.ShouldBe(Value);
    }

    [Fact]
    public void MatchWithResult_ShouldReturnTheNoneBranch_IfOptionIsEmpty()
    {
        var shouldNotBeCalled = Substitute.For<Func<int, int>>();

        var res = NoneT<int>().Match(
            some: shouldNotBeCalled,
            none: () => Value);

        res.ShouldBe(Value);
        shouldNotBeCalled.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public void MatchWithResult_ShouldReturnTheSomeBranch_IfOptionIsNotEmpty()
    {
        var shouldNotBeCalled = Substitute.For<Func<int>>();

        var res = Some(Value).Match(
            some: v => v + 1,
            none: shouldNotBeCalled);

        res.ShouldBe(Value + 1);
        shouldNotBeCalled.DidNotReceiveWithAnyArgs()();
    }

    [Fact]
    public void MatchWithActions_ShouldCallTheNoneBranchOnly_IfOptionIsEmpty()
    {
        var shouldNotBeCalled = Substitute.For<Action<int>>();
        var shouldBeCalled = Substitute.For<Action>();

        NoneT<int>().Match(
            some: shouldNotBeCalled,
            none: shouldBeCalled);

        shouldNotBeCalled.DidNotReceiveWithAnyArgs()(default);
        shouldBeCalled.Received(1)();
    }

    [Fact]
    public void MatchWithActions_ShouldCallTheSomeBranchOnly_IfOptionIsNotEmpty()
    {
        var shouldBeCalled = Substitute.For<Action<int>>();
        var shouldNotBeCalled = Substitute.For<Action>();

        Some(Value).Match(
            some: shouldBeCalled,
            none: shouldNotBeCalled);

        shouldNotBeCalled.DidNotReceiveWithAnyArgs()();
        shouldBeCalled.Received(1)(Value);
    }

    [Fact]
    public void OptionsShouldSupportShortCircuitEvaluationWithOperatorOr()
    {
        var shouldNotBeCalled = Substitute.For<Func<int>>();
        var test = Some(Value) || Some(shouldNotBeCalled());
        test.ShouldContain(Value);
        shouldNotBeCalled.DidNotReceiveWithAnyArgs()();
    }

    [Fact]
    public void OptionsShouldSupportShortCircuitEvaluationWithOperatorAnd()
    {
        var shouldNotBeCalled = Substitute.For<Func<int>>();
        var test = None && Some(shouldNotBeCalled());
        test.ShouldBeEmpty();
        shouldNotBeCalled.DidNotReceiveWithAnyArgs()();
    }

    [Fact]
    public void AsSome_ShouldThrowWithNullArgument()
    {
        Should.Throw<ArgumentNullException>(() =>
        {
            StaticImports.AsSome<object>(null);
        });
    }

    [Fact]
    public void AsSome_ShouldCallSomeWithNotNullArgument()
    {
        string.Empty.AsSome().ShouldBe(Some(string.Empty));
    }
}
