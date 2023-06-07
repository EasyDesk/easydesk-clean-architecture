namespace EasyDesk.CleanArchitecture.Domain.Metamodel;

public abstract class Entity
{
    private List<DomainEvent> _events = new();

    public IEnumerable<DomainEvent> EmittedEvents() => _events
        .Concat(ChildEntities().SelectMany(e => e.EmittedEvents()))
        .ToList();

    protected void EmitEvent(DomainEvent domainEvent)
    {
        _events.Add(domainEvent);
    }

    public IEnumerable<DomainEvent> ConsumeAllEvents()
    {
        var events = _events;
        _events = new();
        return events
            .Concat(ChildEntities().SelectMany(e => e.ConsumeAllEvents()))
            .ToList();
    }

    protected virtual IEnumerable<Entity> ChildEntities()
    {
        return Enumerable.Empty<Entity>();
    }
}
