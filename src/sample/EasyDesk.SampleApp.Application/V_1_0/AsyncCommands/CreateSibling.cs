using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.Commons.Results;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using NodaTime;

namespace EasyDesk.SampleApp.Application.V_1_0.AsyncCommands;

public record CreateSibling(
    string FirstName,
    string LastName,
    LocalDate DateOfBirth,
    string CreatedBy,
    AddressDto Residence) : IOutgoingCommand, IIncomingCommand
{
    public static Duration CreationDelay { get; } = Duration.FromDays(30);

    public static string GetDestination(RoutingContext context) => context.Self;
}

public class CreateSiblingHandler : IHandler<CreateSibling>
{
    private readonly IPersonRepository _personRepository;

    public CreateSiblingHandler(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public Task<Result<Nothing>> Handle(CreateSibling command)
    {
        var person = Person.Create(
            new Name($"{command.FirstName}'s sibling"),
            new Name(command.LastName),
            command.DateOfBirth.PlusYears(1),
            AdminId.From(command.CreatedBy),
            command.Residence.ToDomainObject());
        _personRepository.Save(person);

        return Task.FromResult(Ok);
    }
}
