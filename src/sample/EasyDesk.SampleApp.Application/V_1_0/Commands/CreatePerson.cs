using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
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

public record BirthDateUtcInTheFutureError(LocalDate Date) : ApplicationError
{
    public override string GetDetail() => "The provided date is in the future in UTC.";
}

public record CreatePerson : ICommandRequest<PersonDto>, IAuthorize, IValidate<CreatePerson>
{
    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required LocalDate DateOfBirth { get; init; }

    public required AddressDto Residence { get; init; }

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
    private readonly IClock _clock;

    public CreatePersonHandler(IPersonRepository personRepository, IContextProvider contextProvider, IClock clock)
    {
        _personRepository = personRepository;
        _contextProvider = contextProvider;
        _clock = clock;
    }

    protected override Task<Result<Person>> Process(CreatePerson command)
    {
        if (DateTimeZoneProviders.Tzdb["UTC"].AtLeniently(command.DateOfBirth.AtMidnight()).ToInstant() > _clock.GetCurrentInstant())
        {
            return Task.FromResult<Result<Person>>(new BirthDateUtcInTheFutureError(command.DateOfBirth));
        }
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
