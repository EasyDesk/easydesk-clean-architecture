using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
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
using FluentValidation;
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

    public static void ValidationRules(InlineValidator<CreatePerson> validator)
    {
        validator.RuleFor(x => x.FirstName).MustBeValid().For<Name>();
        validator.RuleFor(x => x.LastName).MustBeValid().For<Name>();
        validator.RuleFor(x => x.Residence).MustBeImplicitlyValid();
    }
}

public class CreatePersonHandler : MappingHandler<CreatePerson, Person, PersonDto>
{
    private readonly IPersonRepository _personRepository;
    private readonly IAgentProvider _agentProvider;
    private readonly IClock _clock;

    public CreatePersonHandler(IPersonRepository personRepository, IAgentProvider agentProvider, IClock clock)
    {
        _personRepository = personRepository;
        _agentProvider = agentProvider;
        _clock = clock;
    }

    protected override Task<Result<Person>> Process(CreatePerson request)
    {
        if (DateTimeZoneProviders.Tzdb["UTC"].AtLeniently(request.DateOfBirth.AtMidnight()).ToInstant() > _clock.GetCurrentInstant())
        {
            return Task.FromResult<Result<Person>>(new BirthDateUtcInTheFutureError(request.DateOfBirth));
        }
        var person = Person.Create(
            new Name(request.FirstName),
            new Name(request.LastName),
            request.DateOfBirth,
            AdminId.From(_agentProvider.RequireAgent().MainIdentity().Id),
            request.Residence.ToDomainObject());
        _personRepository.Save(person);

        return Task.FromResult<Result<Person>>(person);
    }
}
