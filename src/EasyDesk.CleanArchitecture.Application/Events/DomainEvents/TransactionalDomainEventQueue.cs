using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools;
using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.Application.Events.DomainEvents;

public class TransactionalDomainEventQueue : IDomainEventNotifier
{
    private readonly ITransactionManager _transactionManager;
    private readonly IMediator _mediator;
    private readonly Queue<DomainEvent> _eventQueue = new();
    private bool _hasEnqueuedAnyEvents = false;

    public TransactionalDomainEventQueue(ITransactionManager transactionManager, IMediator mediator)
    {
        _transactionManager = transactionManager;
        _mediator = mediator;
    }

    public void Notify(DomainEvent domainEvent)
    {
        if (!_hasEnqueuedAnyEvents)
        {
            _transactionManager.BeforeCommit.Subscribe(Flush);
            _hasEnqueuedAnyEvents = true;
        }
        _eventQueue.Enqueue(domainEvent);
    }

    private async Task Flush(BeforeCommitContext context)
    {
        await HandleEnqueuedEvents()
            .ThenIfFailure(error => context.CancelCommit(error));
    }

    private async Task<Response<Nothing>> HandleEnqueuedEvents()
    {
        while (_eventQueue.TryDequeue(out var nextEvent))
        {
            var result = await _mediator.PublishEvent(nextEvent);
            if (result.IsFailure)
            {
                return result;
            }
        }
        return Ok;
    }
}
