﻿using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.Commons.Options;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate.Events;

namespace EasyDesk.SampleApp.Application.V_1_0.OutgoingEvents;

public record PersonCreated(Guid PersonId) : IOutgoingEvent, IPropagatedEvent<PersonCreated, PersonCreatedEvent>
{
    public static readonly TenantId EmittedWithTenant = new("asdqwe-test");

    public static PersonCreated ToMessage(PersonCreatedEvent domainEvent) => new(domainEvent.Person.Id);

    public static Option<TenantInfo> ToTenant(PersonCreatedEvent domainEvent) => Some(TenantInfo.Tenant(EmittedWithTenant));
}
