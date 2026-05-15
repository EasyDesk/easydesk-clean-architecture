using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public interface IPropagatedEvent<M, D>
    where M : IPropagatedEvent<M, D>, IOutgoingEvent
    where D : DomainEvent
{
    static abstract M ToMessage(D domainEvent);
}
