using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.CleanArchitecture.Application.Messaging.Messages;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.PropagatedEvents;

public record PersonDeleted(Guid PersonId) : IOutgoingEvent;

public class PropagatePersonDeletedEvent : IDomainEventPropagator<PersonDeletedEvent>
{
    public IOutgoingEvent ConvertToMessage(PersonDeletedEvent ev) =>
        new PersonDeleted(ev.Person.Id);
}
