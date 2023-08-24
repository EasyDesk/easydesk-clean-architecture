using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate.Events;

namespace EasyDesk.SampleApp.Application.V_1_0.OutgoingEvents;

public record PersonCreated(Guid PersonId) : IOutgoingEvent, IPropagatedEvent<PersonCreated, PersonCreatedEvent>
{
    public static readonly TenantId EmittedWithTenant = TenantId.New("asdqwe-test");

    public static PersonCreated ToMessage(PersonCreatedEvent ev) => new(ev.Person.Id);

    public static Option<TenantInfo> ToTenant(PersonCreatedEvent ev) => Some(TenantInfo.Tenant(EmittedWithTenant));
}
