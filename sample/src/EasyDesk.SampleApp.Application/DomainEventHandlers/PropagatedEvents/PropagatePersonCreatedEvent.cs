using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using System;

namespace EasyDesk.SampleApp.Application.DomainEventHandlers.PropagatedEvents;

public record PersonCreated(Guid PersonId) : IMessage;

public class PropagatePersonCreatedEvent : DomainEventPropagator<PersonCreatedEvent>
{
    public PropagatePersonCreatedEvent(MessageBroker broker) : base(broker)
    {
    }

    protected override IMessage ConvertToMessage(PersonCreatedEvent ev) =>
        new PersonCreated(ev.Person.Id);
}
