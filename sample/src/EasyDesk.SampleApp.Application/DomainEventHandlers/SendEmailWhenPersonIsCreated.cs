using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.Tools;
using System;
using System.Threading.Tasks;

namespace EasyDesk.SampleApp.Application.DomainEventHandlers;

public record SendPersonCreatedEmail(Guid Id) : IMessage;

public class SendEmailWhenPersonIsCreated : DomainEventHandlerBase<PersonCreatedEvent>
{
    private readonly MessageBroker _messageBroker;

    public SendEmailWhenPersonIsCreated(MessageBroker messageBroker)
    {
        _messageBroker = messageBroker;
    }

    protected override async Task<Response<Nothing>> Handle(PersonCreatedEvent ev)
    {
        await _messageBroker.Send(new SendPersonCreatedEmail(ev.Person.Id));
        return ResponseImports.Ok;
    }
}
