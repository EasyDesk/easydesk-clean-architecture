using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel
{
    public abstract class AggregateRoot<T> : Entity<T, Guid>.ExplicitId
        where T : AggregateRoot<T>
    {
        private IImmutableQueue<IDomainEvent> _events = ImmutableQueue<IDomainEvent>.Empty;

        public AggregateRoot(Guid id)
        {
            Id = id;
        }

        public sealed override Guid Id { get; }

        public IEnumerable<IDomainEvent> EmittedEvents => _events;

        protected void EmitEvent(IDomainEvent domainEvent)
        {
            _events = _events.Enqueue(domainEvent);
        }

        public Option<IDomainEvent> ConsumeEvent()
        {
            if (_events.IsEmpty)
            {
                return None;
            }
            _events = _events.Dequeue(out var consumedEvent);
            return Some(consumedEvent);
        }

        public IEnumerable<IDomainEvent> ConsumeAllEvents()
        {
            return EnumerableUtils.Generate(ConsumeEvent)
                .TakeWhile(ev => ev.IsPresent)
                .Select(ev => ev.Value);
        }
    }
}
