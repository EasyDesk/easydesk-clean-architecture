using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public interface IPropagatedEvent<M, D>
    where M : IPropagatedEvent<M, D>, IOutgoingEvent
    where D : DomainEvent
{
    static abstract M ToMessage(D domainEvent);

    static virtual Option<TenantInfo> ToTenant(D domainEvent) => None;
}
