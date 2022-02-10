using EasyDesk.CleanArchitecture.Application.Mediator.Handlers;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using System;

namespace EasyDesk.SampleApp.Application.DomainEventHandlers.PropagatedEvents;

[RebusAutoSubscribe]
public record PersonCreated(Guid PersonId) : IMessage;

public class PropagatePersonCreatedEvent : IDomainEventPropagator<PersonCreatedEvent>
{
    public IMessage ConvertToMessage(PersonCreatedEvent ev) =>
        new PersonCreated(ev.Person.Id);
}
