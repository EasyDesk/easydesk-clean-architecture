using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.Snapshots;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;
using FluentValidation;

namespace EasyDesk.SampleApp.Application.Commands;

[RequireAnyOf(Permissions.CanEditPets)]
public record CreatePet(string Nickname, Guid PersonId) : ICommandRequest<PetSnapshot>;

public class CreatePetValidator : AbstractValidator<CreatePet>
{
    public CreatePetValidator()
    {
        RuleFor(x => x.Nickname).NotEmpty();
    }
}

public class CreatePetHandler : IHandler<CreatePet, PetSnapshot>
{
    private readonly IPersonRepository _personRepository;
    private readonly IPetRepository _petRepository;
    private readonly IAuditConfigurer _audit;

    public CreatePetHandler(IPersonRepository personRepository, IPetRepository petRepository, IAuditConfigurer audit)
    {
        _personRepository = personRepository;
        _petRepository = petRepository;
        _audit = audit;
    }

    public async Task<Result<PetSnapshot>> Handle(CreatePet request)
    {
        _audit.SetDescription(request.Nickname);

        return await _personRepository.GetById(request.PersonId)
            .ThenOrElseError(Errors.NotFound)
            .ThenMap(_ => Pet.Create(new Name(request.Nickname), request.PersonId))
            .ThenIfSuccessAsync(_petRepository.SaveAndHydrate)
            .ThenMap(PetSnapshot.MapFrom);
    }
}
