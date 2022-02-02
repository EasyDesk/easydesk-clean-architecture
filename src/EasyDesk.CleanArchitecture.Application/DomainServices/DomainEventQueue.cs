using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools;
using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public class DomainEventQueue : IDomainEventNotifier
{
    private readonly Queue<DomainEvent> _eventQueue = new();
    private readonly IMediator _mediator;

    public DomainEventQueue(IMediator mediator)
    {
        _mediator = mediator;
    }

    public void Notify(DomainEvent domainEvent)
    {
        _eventQueue.Enqueue(domainEvent);
    }

    public async Task<Response<Nothing>> Flush()
    {
        while (_eventQueue.TryDequeue(out var ev))
        {
            var result = await _mediator.PublishEvent(ev);
            if (result.IsFailure)
            {
                return result;
            }
        }
        return Ok;
    }
}
