using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Domain.Model;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate;
using FluentValidation;

namespace EasyDesk.SampleApp.Application.Commands;

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
    private readonly IPetRepository _petRepository;

    public CreatePetHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    public async Task<Result<PetSnapshot>> Handle(CreatePet request)
    {
        var pet = Pet.Create(Name.From(request.Nickname), request.PersonId);
        await _petRepository.SaveAndHydrate(pet);
        return PetSnapshot.FromPet(pet);
    }
}
