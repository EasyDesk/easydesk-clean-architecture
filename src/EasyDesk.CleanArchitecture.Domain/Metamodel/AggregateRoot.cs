using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel;

public abstract class AggregateRoot : Entity
{
    private IImmutableQueue<DomainEvent> _events = ImmutableQueue<DomainEvent>.Empty;

    public IEnumerable<DomainEvent> EmittedEvents => _events;

    protected void EmitEvent(DomainEvent domainEvent)
    {
        _events = _events.Enqueue(domainEvent);
    }

    public Option<DomainEvent> ConsumeEvent()
    {
        if (_events.IsEmpty)
        {
            return None;
        }
        _events = _events.Dequeue(out var consumedEvent);
        return Some(consumedEvent);
    }

    public IEnumerable<DomainEvent> ConsumeAllEvents() => EnumerableUtils
        .Generate(ConsumeEvent)
        .TakeWhile(ev => ev.IsPresent)
        .Select(ev => ev.Value);

    public void NotifyCreation() => OnCreation();

    public void NotifyRemoval() => OnRemoval();

    protected virtual void OnCreation()
    {
    }

    protected virtual void OnRemoval()
    {
    }
}
