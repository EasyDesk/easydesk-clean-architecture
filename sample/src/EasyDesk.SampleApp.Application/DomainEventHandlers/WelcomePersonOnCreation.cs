using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.SampleApp.Application.Commands;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;

namespace EasyDesk.SampleApp.Application.DomainEventHandlers;

public class WelcomePersonOnCreation : IDomainEventHandler<PersonCreatedEvent>
{
    private readonly IMessageSender _sender;

    public WelcomePersonOnCreation(IMessageSender sender)
    {
        _sender = sender;
    }

    public async Task<Result<Nothing>> Handle(PersonCreatedEvent ev)
    {
        await _sender.Send(new WelcomePerson(ev.Person.Name));
        return Ok;
    }
}
