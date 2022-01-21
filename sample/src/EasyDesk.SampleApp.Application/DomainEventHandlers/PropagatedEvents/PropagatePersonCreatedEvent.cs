using System;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Messaging.ExternalEvents;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.DomainEventHandlers.PropagatedEvents;

public record PersonCreated(Guid PersonId) : ExternalEvent;

public class PropagatePersonCreatedEvent : DomainEventPropagator<PersonCreatedEvent>
{
    public PropagatePersonCreatedEvent(IExternalEventPublisher publisher) : base(publisher)
    {
    }

    protected override ExternalEvent ConvertToExternalEvent(PersonCreatedEvent ev) =>
        new PersonCreated(ev.Person.Id);
}
