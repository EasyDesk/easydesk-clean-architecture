using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Observables;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EasyDesk.Commons.UnitTests.Observables;

public class SimpleAsyncEventTests
{
    private const int Value = 0;

    private readonly SimpleAsyncEvent<int> _sut = new();

    [Fact]
    public async Task Emit_ShouldNotFail_IfEventHasNoSubscriber()
    {
        await Should.NotThrowAsync(() => _sut.Emit(Value));
    }

    [Fact]
    public async Task Emit_ShouldNotifyHandlersWithTheGivenValueAsync()
    {
        var handler1 = Substitute.For<Action<int>>();
        var handler2 = Substitute.For<Action<int>>();
        _sut.Subscribe(handler1);
        _sut.Subscribe(handler2);

        await _sut.Emit(Value);

        handler1.Received(1)(Value);
        handler2.Received(1)(Value);
    }

    [Fact]
    public async Task Emit_ShouldNotifyAllHandlersInOrderOfSubscriptionAsync()
    {
        var index = 0;

        void InOrderHandler(int expected)
        {
            index.ShouldBe(expected);
            index++;
        }

        Enumerable.Range(0, 10).ForEach(i =>
        {
            _sut.Subscribe(_ => InOrderHandler(i));
        });

        await _sut.Emit(0);
    }

    [Fact]
    public async Task Emit_ShouldNotNotifyUnsubscribedHandlers()
    {
        var handler = Substitute.For<Action<int>>();
        var subscription = _sut.Subscribe(handler);
        subscription.Unsubscribe();

        await _sut.Emit(Value);

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
