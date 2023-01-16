using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.SampleApp.Application.AsyncCommands;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using NodaTime;

namespace EasyDesk.SampleApp.Application.DomainEvents;

public record CreatePassport(Guid PersonId, string FirstName, string LastName, LocalDate DateOfBirth) : IOutgoingCommand
{
    public static string GetDestination(RoutingContext context) => OtherServicesEndpoints.PassportService;
}

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
        await _sender.Send(new CreatePassport(ev.Person.Id, ev.Person.FirstName, ev.Person.LastName, ev.Person.DateOfBirth));
        return Ok;
    }
}
