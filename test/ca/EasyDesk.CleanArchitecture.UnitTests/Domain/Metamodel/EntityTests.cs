using EasyDesk.CleanArchitecture.Domain.Metamodel;
using Shouldly;
using static EasyDesk.Commons.Collections.EnumerableUtils;

namespace EasyDesk.CleanArchitecture.UnitTests.Domain.Metamodel;

public class EntityTests
{
    private record Event(int Value) : DomainEvent;

    private class TestEntity : Entity
    {
        public void Action(int value)
        {
            EmitEvent(new Event(value));
        }
    }

    private class TestEntityTree : TestEntity
    {
        private readonly IEnumerable<Entity> _children;

        public TestEntityTree(IEnumerable<Entity> children)
        {
            _children = children;
        }

        protected override IEnumerable<Entity> ChildEntities() => _children;
    }

    private readonly TestEntity _sut = new();

    private readonly TestEntityTree _sutTree;

    private readonly TestEntity _sutTreeChild;

    public EntityTests()
    {
        _sutTreeChild = new();
        var e = new TestEntity();
        _sutTree = new(new[] { _sutTreeChild, e, _sut, });
    }

    private static Event ToEvent(int value) => new(value);

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
    public void ConsumeAllEvents_ShouldNotBeLazy()
    {
        _sut.Action(1);
        _sut.Action(2);

        _sut.ConsumeAllEvents();

        _sut.EmittedEvents().ShouldBeEmpty();
    }

    [Fact]
    public void ConsumeAllEvents_ShouldConsumeEventsImmediately()
    {
        _sut.Action(1);
        _sut.Action(2);

        var enumerator = _sut.ConsumeAllEvents().GetEnumerator();

        enumerator.MoveNext();
        _sut.EmittedEvents().ShouldBeEmpty();
        enumerator.MoveNext();
        _sut.EmittedEvents().ShouldBeEmpty();
    }

    [Fact]
    public void EmittedEvents_ShouldBeEmpty_IfNoEventsWereEmitted()
    {
        _sut.EmittedEvents().ShouldBeEmpty();
    }

    [Fact]
    public void EmittedEvents_ShouldContainEmittedEventsInFifoOrder()
    {
        _sut.Action(1);
        _sut.Action(2);
        _sut.Action(3);

        _sut.EmittedEvents().ShouldBe(Items(ToEvent(1), ToEvent(2), ToEvent(3)));
    }

    [Fact]
    public void EmittedEvents_ShouldAllowForInterleavingEventEmission()
    {
        _sut.Action(1);
        _sut.Action(2);

        var events = _sut.EmittedEvents();
        _sut.Action(3);

        events.ShouldBe(Items(ToEvent(1), ToEvent(2)));
    }

    [Fact]
    public void EmittedEvents_ShouldContainEventsFromChildren()
    {
        _sutTree.Action(5);
        _sut.Action(1);
        _sutTreeChild.Action(2);

        _sutTree.EmittedEvents().ShouldBe(Items(ToEvent(5), ToEvent(1), ToEvent(2)), ignoreOrder: true);
    }

    [Fact]
    public void ConsumeAllEvents_ShouldConsumeEventsFromChildren()
    {
        _sutTree.Action(5);
        _sut.Action(1);
        _sutTreeChild.Action(2);

        _sutTree.ConsumeAllEvents().ShouldBe(Items(ToEvent(5), ToEvent(1), ToEvent(2)), ignoreOrder: true);
        _sutTree.ConsumeAllEvents().ShouldBeEmpty();
    }
}
