using EasyDesk.CleanArchitecture.Application.Events.ExternalEvents;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using System;

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
