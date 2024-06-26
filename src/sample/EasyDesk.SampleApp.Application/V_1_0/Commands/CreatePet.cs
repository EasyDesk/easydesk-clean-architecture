﻿using EasyDesk.CleanArchitecture.Application.Auditing;
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
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

public record CreatePet(PetInfoDto Pet, Guid PersonId) : ICommandRequest<PetDto>, IAuthorize
{
    public bool IsAuthorized(AuthorizationInfo auth) =>
        auth.HasPermission(Permissions.CanEditPeople);
}

public record PetInfoDto(string Nickname);

public class PetInfoDtoValidator : PimpedAbstractValidator<PetInfoDto>
{
    public PetInfoDtoValidator()
    {
        RuleFor(x => x.Nickname).MustBeValid().For<Name>();
    }
}

public class CreatePetValidator : PimpedAbstractValidator<CreatePet>
{
    public CreatePetValidator()
    {
        RuleFor(x => x.Pet).SetValidator(new PetInfoDtoValidator());
    }
}

public class CreatePetHandler : IHandler<CreatePet, PetDto>
{
    private readonly IPersonRepository _personRepository;
    private readonly IPetRepository _petRepository;
    private readonly IAuditConfigurer _audit;

    public CreatePetHandler(
        IPersonRepository personRepository,
        IPetRepository petRepository,
        IAuditConfigurer audit)
    {
        _personRepository = personRepository;
        _petRepository = petRepository;
        _audit = audit;
    }

    public async Task<Result<PetDto>> Handle(CreatePet request)
    {
        _audit.AddProperty("nickname", request.Pet.Nickname);

        var result = await _personRepository.FindById(request.PersonId)
            .OrElseNotFound()
            .ThenMap(_ => Pet.Create(new Name(request.Pet.Nickname), request.PersonId))
            .ThenIfSuccessAsync(_petRepository.SaveAndHydrate)
            .ThenMap(PetDto.MapFrom);

        return result;
    }
}
