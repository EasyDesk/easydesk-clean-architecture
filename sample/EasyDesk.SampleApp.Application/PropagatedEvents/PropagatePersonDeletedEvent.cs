using EasyDesk.CleanArchitecture.Application.Cqrs.Events;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.PropagatedEvents;

public record PersonDeleted(Guid PersonId) : IOutgoingEvent, IMessage, IPropagatedEvent<PersonDeleted, PersonDeletedEvent>
{
    public static PersonDeleted ToMessage(PersonDeletedEvent ev) => new(ev.Person.Id);
}
