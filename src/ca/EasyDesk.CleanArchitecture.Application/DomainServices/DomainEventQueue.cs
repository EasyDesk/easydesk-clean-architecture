using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

internal class DomainEventQueue : IDomainEventNotifier
{
    private readonly Queue<DomainEvent> _eventQueue = new();
    private readonly DomainEventPublisher _publisher;
    private readonly ISaveChangesHandler _saveChangesHandler;

    public DomainEventQueue(DomainEventPublisher publisher, ISaveChangesHandler saveChangesHandler)
    {
        _publisher = publisher;
        _saveChangesHandler = saveChangesHandler;
    }

    public void Notify(DomainEvent domainEvent)
    {
        _eventQueue.Enqueue(domainEvent);
    }

    public async Task<Result<Nothing>> Flush()
    {
        while (_eventQueue.Count > 0)
        {
            var result = await FlushEventWave().ThenIfSuccessAsync(_ => _saveChangesHandler.SaveChanges());
            if (result.IsFailure)
            {
                return result;
            }
        }
        return Ok;
    }

    private async Task<Result<Nothing>> FlushEventWave()
    {
        var events = _eventQueue.ToList();
        _eventQueue.Clear();
        foreach (var nextEvent in events)
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
