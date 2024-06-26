﻿using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public sealed class PropagateEvent<M, D> : IDomainEventHandler<D>
    where M : IPropagatedEvent<M, D>, IOutgoingEvent
    where D : DomainEvent
{
    private readonly IEventPublisher _publisher;
    private readonly IContextTenantNavigator? _tenantNavigator;

    public PropagateEvent(
        IEventPublisher publisher,
        IContextTenantNavigator? tenantNavigator = null)
    {
        _publisher = publisher;
        _tenantNavigator = tenantNavigator;
    }

    public async Task<Result<Nothing>> Handle(D ev)
    {
        await M.ToTenant(ev).MatchAsync(
            some: async t =>
            {
                using var scope = _tenantNavigator?.NavigateTo(t);
                await Propagate(ev);
            },
            none: async () => await Propagate(ev));
        return Ok;
    }

    private async Task Propagate(D ev) => await _publisher.Publish(M.ToMessage(ev));
}
