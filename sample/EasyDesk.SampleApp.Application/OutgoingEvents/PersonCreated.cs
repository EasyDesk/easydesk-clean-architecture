using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.OutgoingEvents;

public record PersonCreated(Guid PersonId) : IOutgoingEvent, IPropagatedEvent<PersonCreated, PersonCreatedEvent>
{
    public static PersonCreated ToMessage(PersonCreatedEvent ev) => new(ev.Person.Id);
}
