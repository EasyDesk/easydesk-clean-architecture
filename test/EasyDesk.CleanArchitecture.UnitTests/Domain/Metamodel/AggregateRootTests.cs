using EasyDesk.CleanArchitecture.Domain.Metamodel;
using Shouldly;
using static EasyDesk.Tools.Collections.EnumerableUtils;

namespace EasyDesk.CleanArchitecture.UnitTests.Domain.Metamodel;

public class AggregateRootTests
{
    private record Event(int Value) : DomainEvent;

    private class TestAggregateRoot : AggregateRoot
    {
        public TestAggregateRoot()
        {
        }

        public void Action(int value)
        {
            EmitEvent(new Event(value));
        }
    }

    private readonly TestAggregateRoot _sut = new();

    private Event ToEvent(int value) => new(value);

    [Fact]
    public void ConsumeEvent_ShouldReturnNone_IfNoEventsWereEmitted()
    {
        _sut.ConsumeEvent().ShouldBeEmpty();
    }

    [Fact]
    public void ConsumeEvent_ShouldReturnEmittedEventsInFifoOrder()
    {
        _sut.Action(1);
        _sut.Action(2);
        _sut.Action(3);
        _sut.ConsumeEvent().ShouldContain(ToEvent(1));
        _sut.ConsumeEvent().ShouldContain(ToEvent(2));
        _sut.ConsumeEvent().ShouldContain(ToEvent(3));
    }

    [Fact]
    public void ConsumeEvent_ShouldReturnNone_AfterAllEventsAreConsumed()
    {
        _sut.Action(0);
        _sut.Action(0);
        _sut.ConsumeEvent();
        _sut.ConsumeEvent();

        _sut.ConsumeEvent().ShouldBeEmpty();
    }

    [Fact]
    public void ConsumeEvent_ShouldAllowForInterleavingEventEmission()
    {
        _sut.Action(1);
        _sut.Action(2);
        _sut.ConsumeEvent().ShouldContain(ToEvent(1));
        _sut.Action(3);
        _sut.ConsumeEvent().ShouldContain(ToEvent(2));
        _sut.ConsumeEvent().ShouldContain(ToEvent(3));
    }

    [Fact]
    public void ConsumeEvent_ShouldRemoveTheConsumedEventFromTheEmittedEvents()
    {
        _sut.Action(1);
        _sut.Action(2);
        _sut.ConsumeEvent();

        _sut.EmittedEvents.ShouldBe(Items(ToEvent(2)));
    }

    [Fact]
    public void ConsumeAllEvents_ShouldReturnAnEmptySequence_IfNoEventsWereEmitted()
    {
        _sut.ConsumeAllEvents().ShouldBeEmpty();
    }

    [Fact]
    public void ConsumeAllEvents_ShouldReturnEmittedEventsInFifoOrder()
    {
        _sut.Action(1);
        _sut.Action(2);
        _sut.Action(3);

        _sut.ConsumeAllEvents().ShouldBe(Items(ToEvent(1), ToEvent(2), ToEvent(3)));
    }

    [Fact]
    public void ConsumeAllEvents_ShouldBeLazy()
    {
        _sut.Action(1);
        _sut.Action(2);

        _sut.ConsumeAllEvents();

        _sut.EmittedEvents.ShouldNotBeEmpty();
    }

    [Fact]
    public void ConsumeAllEvents_ShouldConsumeEventsAsTheSequenceIsEnumerated()
    {
        _sut.Action(1);
        _sut.Action(2);

        var enumerator = _sut.ConsumeAllEvents().GetEnumerator();

        enumerator.MoveNext();
        _sut.EmittedEvents.ShouldBe(Items(ToEvent(2)));
        enumerator.MoveNext();
        _sut.EmittedEvents.ShouldBeEmpty();
    }

    [Fact]
    public void EmittedEvents_ShouldBeEmpty_IfNoEventsWereEmitted()
    {
        _sut.EmittedEvents.ShouldBeEmpty();
    }

    [Fact]
    public void EmittedEvents_ShouldContainEmittedEventsInFifoOrder()
    {
        _sut.Action(1);
        _sut.Action(2);
        _sut.Action(3);

        _sut.EmittedEvents.ShouldBe(Items(ToEvent(1), ToEvent(2), ToEvent(3)));
    }

    [Fact]
    public void EmittedEvents_ShouldAllowForInterleavingEventEmission()
    {
        _sut.Action(1);
        _sut.Action(2);

        var events = _sut.EmittedEvents;
        _sut.Action(3);

        events.ShouldBe(Items(ToEvent(1), ToEvent(2)));
    }
}
