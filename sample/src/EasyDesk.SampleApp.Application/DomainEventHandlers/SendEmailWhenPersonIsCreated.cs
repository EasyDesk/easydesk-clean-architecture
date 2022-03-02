using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.Tools;
using EasyDesk.Tools.Results;
using System;
using System.Threading.Tasks;
using static EasyDesk.Tools.Results.ResultImports;

namespace EasyDesk.SampleApp.Application.DomainEventHandlers;

public record SendPersonCreatedEmail(Guid Id) : IMessage;

public class SendEmailWhenPersonIsCreated : IDomainEventHandler<PersonCreatedEvent>
{
    private readonly IMessageSender _sender;

    public SendEmailWhenPersonIsCreated(IMessageSender sender)
    {
        _sender = sender;
    }

    public async Task<Result<Nothing>> Handle(PersonCreatedEvent ev)
    {
        await _sender.Send(new SendPersonCreatedEmail(ev.Person.Id));
        return Ok;
    }
}
