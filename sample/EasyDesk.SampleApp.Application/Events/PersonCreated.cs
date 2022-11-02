using EasyDesk.CleanArchitecture.Application.Cqrs.Events;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.Events;

public record PersonCreated(Guid PersonId) : IOutgoingEvent, IMessage, IPropagatedEvent<PersonCreated, PersonCreatedEvent>
{
    public static PersonCreated ToMessage(PersonCreatedEvent ev) => new(ev.Person.Id);
}
