using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public sealed class PropagateEvent<M, D> : IDomainEventHandler<D>
    where M : IPropagatedEvent<M, D>, IOutgoingEvent
    where D : DomainEvent
{
    private readonly IEventPublisher _publisher;

    public PropagateEvent(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task<Result<Nothing>> Handle(D ev)
    {
        await Propagate(ev);
        return Ok;
    }

    private async Task Propagate(D ev) => await _publisher.Publish(M.ToMessage(ev));
}
