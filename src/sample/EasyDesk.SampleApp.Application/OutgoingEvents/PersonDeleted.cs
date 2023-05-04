using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate.Events;

namespace EasyDesk.SampleApp.Application.OutgoingEvents;

public record PersonDeleted(Guid PersonId) : IOutgoingEvent, IMessage, IPropagatedEvent<PersonDeleted, PersonDeletedEvent>
{
    public static PersonDeleted ToMessage(PersonDeletedEvent ev) => new(ev.Person.Id);
}
