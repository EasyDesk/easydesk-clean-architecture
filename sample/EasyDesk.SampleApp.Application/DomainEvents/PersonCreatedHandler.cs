using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.SampleApp.Application.AsyncCommands;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.DomainEvents;

public class PersonCreatedHandler : IDomainEventHandler<PersonCreatedEvent>
{
    private readonly ICommandSender _sender;

    public PersonCreatedHandler(ICommandSender sender)
    {
        _sender = sender;
    }

    public async Task<Result<Nothing>> Handle(PersonCreatedEvent ev)
    {
        await _sender.Send(new CreateBestFriend(ev.Person.Id, ev.Person.FirstName));
        return Ok;
    }
}
