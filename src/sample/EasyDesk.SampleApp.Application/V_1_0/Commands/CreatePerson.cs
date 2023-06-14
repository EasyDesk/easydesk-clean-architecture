using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Application.Validation;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using FluentValidation;
using NodaTime;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

[RequireAnyOf(Permissions.CanEditPeople)]
public record CreatePerson(
    string FirstName,
    string LastName,
    LocalDate DateOfBirth,
    AddressDto Residence) : ICommandRequest<PersonDto>;

public class CreatePersonValidator : PimpedAbstractValidator<CreatePerson>
{
    public CreatePersonValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Residence).SetValidator(new AddressDtoValidator());
    }
}

public class CreatePersonHandler : MappingHandler<CreatePerson, Person, PersonDto>
{
    private readonly IPersonRepository _personRepository;
    private readonly IContextProvider _contextProvider;
    private readonly ITenantNavigator _tenantNavigator;

    public CreatePersonHandler(IPersonRepository personRepository, IContextProvider contextProvider, ITenantNavigator tenantNavigator)
    {
        _personRepository = personRepository;
        _contextProvider = contextProvider;
        _tenantNavigator = tenantNavigator;
    }

    protected override Task<Result<Person>> Process(CreatePerson command)
    {
        var person = Person.Create(
            new Name(command.FirstName),
            new Name(command.LastName),
            command.DateOfBirth,
            AdminId.From(_contextProvider.RequireIdentity().Id),
            command.Residence.ToDomainObject());
        _personRepository.Save(person);

        _tenantNavigator.MoveToTenant(TenantId.New("a-tenant-that-does-not-exists"));
        return Task.FromResult<Result<Person>>(person);
    }
}
