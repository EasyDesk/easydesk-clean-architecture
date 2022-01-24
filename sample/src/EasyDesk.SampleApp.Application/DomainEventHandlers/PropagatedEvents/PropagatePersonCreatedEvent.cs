using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Messaging.Sender;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using System;

namespace EasyDesk.SampleApp.Application.DomainEventHandlers.PropagatedEvents;

public record PersonCreated(Guid PersonId) : IMessage;

public class PropagatePersonCreatedEvent : DomainEventPropagator<PersonCreatedEvent>
{
    public PropagatePersonCreatedEvent(IMessageSender publisher) : base(publisher)
    {
    }

    protected override IMessage ConvertToMessage(PersonCreatedEvent ev) =>
        new PersonCreated(ev.Person.Id);
}
