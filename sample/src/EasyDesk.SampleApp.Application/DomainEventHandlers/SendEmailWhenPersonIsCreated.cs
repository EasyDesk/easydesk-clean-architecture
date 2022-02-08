using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Results;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.Tools;
using System;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Domain.Metamodel.Results.ResultImports;

namespace EasyDesk.SampleApp.Application.DomainEventHandlers;

public record SendPersonCreatedEmail(Guid Id) : IMessage;

public class SendEmailWhenPersonIsCreated : IDomainEventHandler<PersonCreatedEvent>
{
    private readonly MessageBroker _messageBroker;

    public SendEmailWhenPersonIsCreated(MessageBroker messageBroker)
    {
        _messageBroker = messageBroker;
    }

    public async Task<Result<Nothing>> Handle(PersonCreatedEvent ev)
    {
        await _messageBroker.Send(new SendPersonCreatedEmail(ev.Person.Id));
        return Ok;
    }
}
