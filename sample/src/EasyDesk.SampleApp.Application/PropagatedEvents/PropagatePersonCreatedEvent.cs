using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.PropagatedEvents;

[RebusAutoSubscribe]
public record PersonCreated(Guid PersonId) : IOutgoingEvent, IIncomingEvent;

public class PropagatePersonCreatedEvent : IDomainEventPropagator<PersonCreatedEvent>
{
    public IOutgoingEvent ConvertToMessage(PersonCreatedEvent ev) =>
        new PersonCreated(ev.Person.Id);
}
