using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.CleanArchitecture.Application.Messaging.Messages;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.PropagatedEvents;

public record PersonCreated(Guid PersonId) : IOutgoingEvent;

public class PropagatePersonCreatedEvent : IDomainEventPropagator<PersonCreatedEvent>
{
    public IOutgoingEvent ConvertToMessage(PersonCreatedEvent ev) =>
        new PersonCreated(ev.Person.Id);
}
