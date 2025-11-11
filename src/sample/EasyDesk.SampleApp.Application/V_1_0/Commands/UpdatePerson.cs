using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Validation;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Values.Validation;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.Commons.Results;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using FluentValidation;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

public record UpdatePerson(
    Guid Id,
    string FirstName,
    string LastName,
    AddressDto Residence) : ICommandRequest<PersonDto>, IAuthorize, IValidate<UpdatePerson>
{
    public bool IsAuthorized(AuthorizationInfo auth) =>
        auth.HasPermission(Permissions.CanEditPeople);

    public static void ValidationRules(InlineValidator<UpdatePerson> validator)
    {
        validator.RuleFor(x => x.FirstName).MustBeValid().For<Name>();
        validator.RuleFor(x => x.LastName).MustBeValid().For<Name>();
        validator.RuleFor(x => x.Residence).MustBeImplicitlyValid();
    }
}

public class UpdatePersonHandler : IHandler<UpdatePerson, PersonDto>
{
    private readonly IPersonRepository _personRepository;

    public UpdatePersonHandler(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task<Result<PersonDto>> Handle(UpdatePerson request)
    {
        return await _personRepository
            .FindById(request.Id)
            .OrElseNotFound()
            .ThenIfSuccess(person =>
            {
                person.FirstName = new(request.FirstName);
                person.LastName = new(request.LastName);
                person.Residence = request.Residence.ToDomainObject();
            })
            .ThenIfSuccess(_personRepository.Save)
            .ThenMap(PersonDto.MapFrom);
    }
}
