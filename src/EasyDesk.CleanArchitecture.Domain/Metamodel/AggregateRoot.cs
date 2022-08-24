using EasyDesk.Tools.Collections;
using System.Collections.Immutable;

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
