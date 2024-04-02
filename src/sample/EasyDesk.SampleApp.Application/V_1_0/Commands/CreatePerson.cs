using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.CleanArchitecture.Application.Validation;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values.Validation;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.Commons.Results;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using NodaTime;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

public record CreatePerson(
    string FirstName,
    string LastName,
    LocalDate DateOfBirth,
    AddressDto Residence) : ICommandRequest<PersonDto>, IAuthorize, IValidate<CreatePerson>
{
    public bool IsAuthorized(AuthorizationInfo auth) =>
        auth.HasPermission(Permissions.CanEditPeople);

    public static void ValidationRules(PimpedInlineValidator<CreatePerson> validator)
    {
        validator.RuleFor(x => x.FirstName).MustBeValid().For<Name>();
        validator.RuleFor(x => x.LastName).MustBeValid().For<Name>();
        validator.RuleFor(x => x.Residence).MustBeImplicitlyValid();
    }
}

public class CreatePersonHandler : MappingHandler<CreatePerson, Person, PersonDto>
{
    private readonly IPersonRepository _personRepository;
    private readonly IContextProvider _contextProvider;

    public CreatePersonHandler(IPersonRepository personRepository, IContextProvider contextProvider)
    {
        _personRepository = personRepository;
        _contextProvider = contextProvider;
    }

    protected override Task<Result<Person>> Process(CreatePerson command)
    {
        var person = Person.Create(
            new Name(command.FirstName),
            new Name(command.LastName),
            command.DateOfBirth,
            AdminId.From(_contextProvider.RequireAgent().MainIdentity().Id),
            command.Residence.ToDomainObject());
        _personRepository.Save(person);

        return Task.FromResult<Result<Person>>(person);
    }
}
