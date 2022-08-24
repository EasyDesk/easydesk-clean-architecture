using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public class DomainEventQueue : IDomainEventNotifier
{
    private readonly Queue<DomainEvent> _eventQueue = new();
    private readonly IDomainEventPublisher _publisher;

    public DomainEventQueue(IDomainEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public void Notify(DomainEvent domainEvent)
    {
        _eventQueue.Enqueue(domainEvent);
    }

    public async Task<Result<Nothing>> Flush()
    {
        while (_eventQueue.TryDequeue(out var nextEvent))
        {
            var result = await _publisher.Publish(nextEvent);
            if (result.IsFailure)
            {
                return result;
            }
        }
        return Ok;
    }
}
