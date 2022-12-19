using EasyDesk.CleanArchitecture.Application.ContextProvider;
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
    private readonly IUserInfoProvider _contextProvider;

    public CreatePersonHandler(IPersonRepository personRepository, IUserInfoProvider contextProvider)
    {
        _personRepository = personRepository;
        _contextProvider = contextProvider;
    }

    protected override Task<Result<Person>> Process(CreatePerson command)
    {
        var person = Person.Create(
            Name.From(command.FirstName),
            Name.From(command.LastName),
            command.DateOfBirth,
            AdminId.From(_contextProvider.RequireUserInfo().UserId));
        _personRepository.Save(person);
        return Task.FromResult<Result<Person>>(person);
    }
}
