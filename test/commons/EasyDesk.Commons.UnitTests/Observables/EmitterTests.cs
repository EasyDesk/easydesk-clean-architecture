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
        _sut.Observable.Subscribe(handler1);
        _sut.Observable.Subscribe(handler2);

        _sut.Emit(Value);

        handler1.Received(1)(Value);
        handler2.Received(1)(Value);
    }

    [Fact]
    public void Emit_ShouldNotifyAllHandlersInOrderOfSubscription()
    {
        var index = 0;

        void InOrderHandler(int expected)
        {
            index.ShouldBe(expected);
            index++;
        }

        Enumerable.Range(0, 10).ForEach(i =>
        {
            _sut.Observable.Subscribe(_ => InOrderHandler(i));
        });

        _sut.Emit(0);
    }

    [Fact]
    public void Emit_ShouldNotNotifyUnsubscribedHandlers()
    {
        var handler = Substitute.For<Action<int>>();
        var subscription = _sut.Observable.Subscribe(handler);
        subscription.Unsubscribe();

        _sut.Emit(Value);

        handler.DidNotReceiveWithAnyArgs()(default);
    }

    [Fact]
    public void Unsubscribe_ShouldFail_IfCalledMultipleTimes()
    {
        var subscription = _sut.Observable.Subscribe(_ => { });
        subscription.Unsubscribe();

        Should.Throw<InvalidOperationException>(subscription.Unsubscribe);
    }
}
