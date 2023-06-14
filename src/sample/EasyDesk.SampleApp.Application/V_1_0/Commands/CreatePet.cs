using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Application.Validation;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;
using FluentValidation;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

public record CreatePet(PetInfoDto Pet, Guid PersonId) : ICommandRequest<PetDto>, IAuthorize
{
    public bool IsAuthorized(AuthorizationInfo auth) =>
        auth.HasPermission(Permissions.CanEditPeople);
}

public record PetInfoDto(string Nickname);

public class CreatePetValidator : PimpedAbstractValidator<CreatePet>
{
    public CreatePetValidator()
    {
        RuleFor(x => x.Pet.Nickname).NotEmpty();
    }
}

public class CreatePetHandler : IHandler<CreatePet, PetDto>
{
    private readonly IPersonRepository _personRepository;
    private readonly IPetRepository _petRepository;
    private readonly IAuditConfigurer _audit;
    private readonly ITenantNavigator _tenantNavigator;

    public CreatePetHandler(
        IPersonRepository personRepository,
        IPetRepository petRepository,
        IAuditConfigurer audit,
        ITenantNavigator tenantNavigator)
    {
        _personRepository = personRepository;
        _petRepository = petRepository;
        _audit = audit;
        _tenantNavigator = tenantNavigator;
    }

    public async Task<Result<PetDto>> Handle(CreatePet request)
    {
        _audit.AddProperty("nickname", request.Pet.Nickname);

        var result = await _personRepository.FindById(request.PersonId)
            .OrElseNotFound()
            .ThenMap(_ => Pet.Create(new Name(request.Pet.Nickname), request.PersonId))
            .ThenIfSuccessAsync(_petRepository.SaveAndHydrate)
            .ThenMap(PetDto.MapFrom);

        _tenantNavigator.MoveToTenant(TenantId.New("a-tenant-that-should-not-exist"));

        return result;
    }
}
