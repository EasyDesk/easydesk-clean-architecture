using EasyDesk.Tools.Options;
using System;
using System.Collections.Generic;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel
{
    public abstract class AggregateRoot<T> : Entity<T, Guid>.ExplicitId, IDomainEventBuffer
        where T : AggregateRoot<T>
    {
        private readonly Queue<IDomainEvent> _events = new();

        public AggregateRoot(Guid id)
        {
            Id = id;
        }

        public sealed override Guid Id { get; }

        protected void EmitEvent(IDomainEvent domainEvent)
        {
            _events.Enqueue(domainEvent);
        }

        public Option<IDomainEvent> ConsumeEvent()
        {
            return FromTryConstruct<IDomainEvent>(_events.TryDequeue);
        }
    }
}
