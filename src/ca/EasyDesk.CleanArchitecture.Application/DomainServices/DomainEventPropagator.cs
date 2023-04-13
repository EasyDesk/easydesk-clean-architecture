using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public sealed class DomainEventPropagator<M, D> : IDomainEventHandler<D>
    where M : IPropagatedEvent<M, D>, IOutgoingEvent
    where D : DomainEvent
{
    private readonly IEventPublisher _publisher;

    public DomainEventPropagator(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task<Result<Nothing>> Handle(D ev)
    {
        await _publisher.Publish(M.ToMessage(ev));
        return Ok;
    }
}
