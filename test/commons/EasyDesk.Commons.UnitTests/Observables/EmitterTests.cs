using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Observables;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EasyDesk.Commons.UnitTests.Observables;

public class EmitterTests
{
    private const int Value = 0;

    private readonly Emitter<int> _sut = new();

    [Fact]
    public void Emit_ShouldNotFail_IfEventHasNoSubscriber()
    {
        Should.NotThrow(() => _sut.Emit(Value));
    }

    [Fact]
    public void Emit_ShouldNotifyHandlersWithTheGivenValue()
    {
        var handler1 = Substitute.For<Action<int>>();
        var handler2 = Substitute.For<Action<int>>();
        _sut.Subscribe(handler1);
        _sut.Subscribe(handler2);

        _sut.Emit(Value);

        handler1.Received(1)(Value);
        handler2.Received(1)(Value);
    }

    [Fact]
    public void Emit_ShouldNotifyAllHandlersInOrderOfSubscription()
    {
        var range = Enumerable.Range(0, 10);
        var action = Substitute.For<Action<int>>();

        range.ForEach(i => _sut.Subscribe(_ => action(i)));

        _sut.Emit(0);

        Received.InOrder(() => range.ForEach(action));
    }

    [Fact]
    public void Emit_ShouldNotNotifyUnsubscribedHandlers()
    {
        var handler = Substitute.For<Action<int>>();
        var subscription = _sut.Subscribe(handler);
        subscription.Unsubscribe();

        _sut.Emit(Value);

        handler.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public void Unsubscribe_ShouldFail_IfCalledMultipleTimes()
    {
        var subscription = _sut.Subscribe(_ => { });
        subscription.Unsubscribe();

        Should.Throw<InvalidOperationException>(subscription.Unsubscribe);
    }
}
