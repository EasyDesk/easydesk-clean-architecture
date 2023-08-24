using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public sealed class DomainEventPropagator<M, D> : IDomainEventHandler<D>
    where M : IPropagatedEvent<M, D>, IOutgoingEvent
    where D : DomainEvent
{
    private readonly IEventPublisher _publisher;
    private readonly IContextTenantNavigator? _tenantNavigator;

    public DomainEventPropagator(
        IEventPublisher publisher,
        IContextTenantNavigator? tenantNavigator = null)
    {
        _publisher = publisher;
        _tenantNavigator = tenantNavigator;
    }

    public async Task<Result<Nothing>> Handle(D ev)
    {
        _tenantNavigator?.MoveTo(M.ToTenant(ev));
        await _publisher.Publish(M.ToMessage(ev));
        return Ok;
    }
}
