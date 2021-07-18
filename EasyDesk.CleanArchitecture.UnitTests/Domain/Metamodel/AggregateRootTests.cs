using EasyDesk.CleanArchitecture.Domain.Metamodel;
using Shouldly;
using System;
using Xunit;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.UnitTests.Domain.Metamodel
{
    public class AggregateRootTests
    {
        private record Event(int Value) : IDomainEvent;

        private class TestAggregateRoot : AggregateRoot<TestAggregateRoot>
        {
            public TestAggregateRoot() : base(Guid.NewGuid())
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
            _sut.ConsumeEvent().ShouldBe(None);
        }

        [Fact]
        public void ConsumeEvent_ShouldReturnEmittedEventsInFifoOrder()
        {
            _sut.Action(1);
            _sut.Action(2);
            _sut.Action(3);
            _sut.ConsumeEvent().ShouldBe(Some(ToEvent(1)));
            _sut.ConsumeEvent().ShouldBe(Some(ToEvent(2)));
            _sut.ConsumeEvent().ShouldBe(Some(ToEvent(3)));
        }

        [Fact]
        public void ConsumeEvent_ShouldReturnNone_AfterAllEventsAreConsumed()
        {
            _sut.Action(0);
            _sut.Action(0);
            _sut.ConsumeEvent();
            _sut.ConsumeEvent();

            _sut.ConsumeEvent().ShouldBe(None);
        }

        [Fact]
        public void ConsumeEvent_ShouldAllowForInterleavingEventEmission()
        {
            _sut.Action(1);
            _sut.Action(2);
            _sut.ConsumeEvent().ShouldBe(Some(ToEvent(1)));
            _sut.Action(3);
            _sut.ConsumeEvent().ShouldBe(Some(ToEvent(2)));
            _sut.ConsumeEvent().ShouldBe(Some(ToEvent(3)));
        }
    }
}
