using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Results;
using EasyDesk.SampleApp.Application.V_1_0.AsyncCommands;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate.Events;

namespace EasyDesk.SampleApp.Application.DomainEvents;

public class ScheduleCommandOnPersonCreated : IDomainEventHandler<PersonCreatedEvent>
{
    private readonly ICommandSender _commandSender;

    public ScheduleCommandOnPersonCreated(ICommandSender commandSender)
    {
        _commandSender = commandSender;
    }

    public async Task<Result<Nothing>> Handle(PersonCreatedEvent ev)
    {
        await _commandSender.Defer(CreateSibling.CreationDelay, new CreateSibling(
            FirstName: ev.Person.FirstName,
            LastName: ev.Person.LastName,
            DateOfBirth: ev.Person.DateOfBirth,
            CreatedBy: ev.Person.CreatedBy,
            Residence: AddressDto.MapFrom(ev.Person.Residence)));

        return Ok;
    }
}
