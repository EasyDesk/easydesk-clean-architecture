using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.SampleApp.Application.Commands;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.DomainEventHandlers;

public class WelcomePersonOnCreation : IDomainEventHandler<PersonCreatedEvent>
{
    private readonly ICommandSender _sender;

    public WelcomePersonOnCreation(ICommandSender sender)
    {
        _sender = sender;
    }

    public async Task<Result<Nothing>> Handle(PersonCreatedEvent ev)
    {
        await _sender.Send(new WelcomePerson(ev.Person.Name));
        return Ok;
    }
}
