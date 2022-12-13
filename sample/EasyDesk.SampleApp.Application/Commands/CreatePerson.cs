using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using FluentValidation;
using NodaTime;

namespace EasyDesk.SampleApp.Application.Commands;

public record CreatePerson(
    string FirstName,
    string LastName,
    LocalDate DateOfBirth) : ICommandRequest<PersonSnapshot>, IOverrideMultitenantPolicy
{
    public MultitenantPolicy MultitenantPolicy => MultitenantPolicies.RequireAnyTenant();
}

public class CreatePersonValidator : AbstractValidator<CreatePerson>
{
    public CreatePersonValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
    }
}

public class CreatePersonHandler : MappingHandler<CreatePerson, Person, PersonSnapshot>
{
    private readonly IPersonRepository _personRepository;

    public CreatePersonHandler(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    protected override async Task<Result<Person>> Process(CreatePerson command)
    {
        var person = Person.Create(Name.From(command.FirstName), Name.From(command.LastName), command.DateOfBirth);
        await _personRepository.Save(person);
        return person;
    }
}
